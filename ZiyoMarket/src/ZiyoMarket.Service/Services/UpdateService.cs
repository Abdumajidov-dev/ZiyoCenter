using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Service.DTOs.Update;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Service for managing application auto-update system
/// </summary>
public class UpdateService : IUpdateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateService> _logger;
    private readonly string _storagePath;

    public UpdateService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "updates");

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }

    public async Task<Result<UpdateInfoDto>> CheckForUpdatesAsync(CheckUpdateRequest request)
    {
        try
        {
            // Get latest version for the channel and platform
            var latestVersion = await _unitOfWork.AppVersions.Table
                .Where(v => v.Channel == request.Channel
                         && v.Platform == request.Platform
                         && v.IsActive
                         && v.DeletedAt == null)
                .OrderByDescending(v => v.ReleaseDate)
                .FirstOrDefaultAsync();

            if (latestVersion == null)
            {
                return Result<UpdateInfoDto>.NotFound("No versions available for this channel");
            }

            // Check if update is available
            bool updateAvailable = latestVersion.IsNewerThan(request.CurrentVersion);

            var updateInfo = new UpdateInfoDto
            {
                UpdateAvailable = updateAvailable,
                LatestVersion = latestVersion.VersionNumber,
                CurrentVersion = request.CurrentVersion,
                ReleaseDate = latestVersion.ReleaseDate,
                DownloadUrl = latestVersion.DownloadUrl ?? $"/api/update/download/{latestVersion.Id}",
                ReleaseNotes = latestVersion.ReleaseNotes,
                IsCritical = latestVersion.IsCritical,
                FileSize = latestVersion.FileSize,
                Sha256Hash = latestVersion.Sha256Hash,
                MinVersionRequired = latestVersion.MinVersionRequired,
                FileName = latestVersion.FileName
            };

            _logger.LogInformation(
                "Update check: Client={ClientVersion}, Latest={LatestVersion}, Available={Available}",
                request.CurrentVersion, latestVersion.VersionNumber, updateAvailable);

            return Result<UpdateInfoDto>.Success(updateInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for updates");
            return Result<UpdateInfoDto>.InternalError("Error checking for updates");
        }
    }

    public async Task<Result<AppVersionDto>> GetLatestVersionAsync(string channel = "stable", string platform = "windows")
    {
        try
        {
            var version = await _unitOfWork.AppVersions.Table
                .Where(v => v.Channel == channel
                         && v.Platform == platform
                         && v.IsActive
                         && v.DeletedAt == null)
                .OrderByDescending(v => v.ReleaseDate)
                .FirstOrDefaultAsync();

            if (version == null)
                return Result<AppVersionDto>.NotFound("No versions found");

            var dto = _mapper.Map<AppVersionDto>(version);
            dto.DownloadCount = await _unitOfWork.UpdateDownloads
                .CountAsync(d => d.VersionId == version.Id);

            return Result<AppVersionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest version");
            return Result<AppVersionDto>.InternalError("Error retrieving version");
        }
    }

    public async Task<Result<List<AppVersionDto>>> GetAllReleasesAsync(string? channel = null, string? platform = null)
    {
        try
        {
            var query = _unitOfWork.AppVersions.Table
                .Where(v => v.IsActive && v.DeletedAt == null);

            if (!string.IsNullOrEmpty(channel))
                query = query.Where(v => v.Channel == channel);

            if (!string.IsNullOrEmpty(platform))
                query = query.Where(v => v.Platform == platform);

            var versions = await query
                .OrderByDescending(v => v.ReleaseDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<AppVersionDto>>(versions);

            // Add download counts
            foreach (var dto in dtos)
            {
                dto.DownloadCount = await _unitOfWork.UpdateDownloads
                    .CountAsync(d => d.VersionId == dto.Id);
            }

            return Result<List<AppVersionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting releases");
            return Result<List<AppVersionDto>>.InternalError("Error retrieving releases");
        }
    }

    public async Task<Result<AppVersionDto>> GetVersionByIdAsync(int id)
    {
        try
        {
            var version = await _unitOfWork.AppVersions.GetByIdAsync(id);
            if (version == null || version.DeletedAt != null)
                return Result<AppVersionDto>.NotFound("Version not found");

            var dto = _mapper.Map<AppVersionDto>(version);
            dto.DownloadCount = await _unitOfWork.UpdateDownloads
                .CountAsync(d => d.VersionId == id);

            return Result<AppVersionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version by ID");
            return Result<AppVersionDto>.InternalError("Error retrieving version");
        }
    }

    public async Task<Result<AppVersionDto>> CreateVersionAsync(CreateAppVersionRequest request, int createdBy)
    {
        try
        {
            // Validate version format
            if (!IsValidVersionFormat(request.VersionNumber))
                return Result<AppVersionDto>.BadRequest("Invalid version format. Use SemVer (e.g., 1.2.0)");

            // Check if version already exists
            var exists = await _unitOfWork.AppVersions.AnyAsync(v =>
                v.VersionNumber == request.VersionNumber &&
                v.Platform == request.Platform &&
                v.Channel == request.Channel &&
                v.DeletedAt == null);

            if (exists)
                return Result<AppVersionDto>.Conflict("Version already exists");

            // Create version entity
            var version = new AppVersion
            {
                VersionNumber = request.VersionNumber,
                ReleaseDate = DateTime.UtcNow,
                ReleaseNotes = request.ReleaseNotes,
                IsCritical = request.IsCritical,
                Channel = request.Channel,
                MinVersionRequired = request.MinVersionRequired,
                Platform = request.Platform,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Handle file upload if provided
            if (request.File != null && request.File.Length > 0)
            {
                var fileResult = await SaveFileAsync(request.File, version.VersionNumber, version.Platform);
                if (fileResult == null)
                    return Result<AppVersionDto>.BadRequest("Failed to save file");

                version.FileName = fileResult.Value.FileName;
                version.FileSize = fileResult.Value.FileSize;
                version.Sha256Hash = fileResult.Value.Sha256Hash;
                version.DownloadUrl = $"/api/update/download/{version.VersionNumber}";
            }

            await _unitOfWork.AppVersions.InsertAsync(version);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<AppVersionDto>(version);
            dto.DownloadCount = 0;

            _logger.LogInformation("Created new version: {Version}", version.VersionNumber);

            return Result<AppVersionDto>.Success(dto, "Version created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating version");
            return Result<AppVersionDto>.InternalError("Error creating version");
        }
    }

    public async Task<Result<AppVersionDto>> UpdateVersionAsync(int id, CreateAppVersionRequest request, int updatedBy)
    {
        try
        {
            var version = await _unitOfWork.AppVersions.GetByIdAsync(id);
            if (version == null || version.DeletedAt != null)
                return Result<AppVersionDto>.NotFound("Version not found");

            // Update properties
            version.ReleaseNotes = request.ReleaseNotes;
            version.IsCritical = request.IsCritical;
            version.MinVersionRequired = request.MinVersionRequired;
            version.UpdatedBy = updatedBy;
            version.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            // Handle file upload if provided
            if (request.File != null && request.File.Length > 0)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(version.FileName))
                {
                    DeleteFile(version.FileName);
                }

                var fileResult = await SaveFileAsync(request.File, version.VersionNumber, version.Platform);
                if (fileResult == null)
                    return Result<AppVersionDto>.BadRequest("Failed to save file");

                version.FileName = fileResult.Value.FileName;
                version.FileSize = fileResult.Value.FileSize;
                version.Sha256Hash = fileResult.Value.Sha256Hash;
            }

            await _unitOfWork.AppVersions.Update(version, id);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<AppVersionDto>(version);
            dto.DownloadCount = await _unitOfWork.UpdateDownloads.CountAsync(d => d.VersionId == id);

            _logger.LogInformation("Updated version: {Version}", version.VersionNumber);

            return Result<AppVersionDto>.Success(dto, "Version updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating version");
            return Result<AppVersionDto>.InternalError("Error updating version");
        }
    }

    public async Task<Result> DeleteVersionAsync(int id, int deletedBy)
    {
        try
        {
            var version = await _unitOfWork.AppVersions.GetByIdAsync(id);
            if (version == null || version.DeletedAt != null)
                return Result.NotFound("Version not found");

            // Soft delete
            version.Delete();
            version.IsActive = false;

            await _unitOfWork.AppVersions.Update(version, id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted version: {Version}", version.VersionNumber);

            return Result.Success("Version deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting version");
            return Result.InternalError("Error deleting version");
        }
    }

    public async Task<Result> RecordDownloadAsync(int versionId, string? clientVersion, string? ipAddress, string? userAgent)
    {
        try
        {
            var download = new UpdateDownload
            {
                VersionId = versionId,
                DownloadDate = DateTime.UtcNow,
                ClientVersion = clientVersion,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = true,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await _unitOfWork.UpdateDownloads.InsertAsync(download);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Recorded download for version ID: {VersionId}", versionId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording download");
            // Don't fail the download if analytics fail
            return Result.Success();
        }
    }

    public async Task<Result<UpdateStatisticsDto>> GetStatisticsAsync()
    {
        try
        {
            var totalDownloads = await _unitOfWork.UpdateDownloads.CountAsync();
            var totalVersions = await _unitOfWork.AppVersions.CountAsync(v => v.IsActive && v.DeletedAt == null);

            var latestVersion = await _unitOfWork.AppVersions.Table
                .Where(v => v.IsActive && v.DeletedAt == null)
                .OrderByDescending(v => v.ReleaseDate)
                .Select(v => v.VersionNumber)
                .FirstOrDefaultAsync();

            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);

            var downloadsToday = await _unitOfWork.UpdateDownloads.Table
                .CountAsync(d => d.DownloadDate.Date == today);

            var downloadsThisWeek = await _unitOfWork.UpdateDownloads.Table
                .CountAsync(d => d.DownloadDate >= weekAgo);

            var downloadsThisMonth = await _unitOfWork.UpdateDownloads.Table
                .CountAsync(d => d.DownloadDate >= monthAgo);

            // Top versions by download count
            var topVersions = await _unitOfWork.UpdateDownloads.Table
                .Include(d => d.Version)
                .Where(d => d.Version != null && d.Version.DeletedAt == null)
                .GroupBy(d => d.Version!.VersionNumber)
                .Select(g => new VersionDownloadStat
                {
                    VersionNumber = g.Key,
                    DownloadCount = g.Count(),
                    Percentage = totalDownloads > 0 ? (g.Count() * 100.0 / totalDownloads) : 0
                })
                .OrderByDescending(v => v.DownloadCount)
                .Take(10)
                .ToListAsync();

            var statistics = new UpdateStatisticsDto
            {
                TotalDownloads = totalDownloads,
                TotalVersions = totalVersions,
                LatestVersion = latestVersion,
                DownloadsToday = downloadsToday,
                DownloadsThisWeek = downloadsThisWeek,
                DownloadsThisMonth = downloadsThisMonth,
                TopVersions = topVersions
            };

            return Result<UpdateStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return Result<UpdateStatisticsDto>.InternalError("Error retrieving statistics");
        }
    }

    public async Task<Result<string>> GetDownloadFilePathAsync(int versionId)
    {
        try
        {
            var version = await _unitOfWork.AppVersions.GetByIdAsync(versionId);
            if (version == null || version.DeletedAt != null)
                return Result<string>.NotFound("Version not found");

            if (string.IsNullOrEmpty(version.FileName))
                return Result<string>.NotFound("File not found");

            var filePath = Path.Combine(_storagePath, version.FileName);
            if (!File.Exists(filePath))
                return Result<string>.NotFound("File not found on server");

            return Result<string>.Success(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file path");
            return Result<string>.InternalError("Error retrieving file");
        }
    }

    // Helper methods
    private bool IsValidVersionFormat(string version)
    {
        // Basic SemVer validation: Major.Minor.Patch or Major.Minor.Patch-prerelease
        var parts = version.Split('-')[0].Split('.');
        return parts.Length == 3 && parts.All(p => int.TryParse(p, out _));
    }

    private async Task<(string FileName, long FileSize, string Sha256Hash)?> SaveFileAsync(
        IFormFile file, string version, string platform)
    {
        try
        {
            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{platform}_{version}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_storagePath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Calculate SHA256 hash
            string hash;
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    var hashBytes = await sha256.ComputeHashAsync(fileStream);
                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }

            return (fileName, file.Length, hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file");
            return null;
        }
    }

    private void DeleteFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_storagePath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted file: {FileName}", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting file: {FileName}", fileName);
        }
    }
}
