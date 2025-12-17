using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Service.DTOs.Sellers;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class SellerService : ISellerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SellerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SellerDetailDto>> GetSellerByIdAsync(int sellerId)
    {
        try
        {
            var seller = await _unitOfWork.Sellers
                .SelectAsync(s => s.Id == sellerId && s.DeletedAt == null);

            if (seller == null)
                return Result<SellerDetailDto>.NotFound("Seller not found");

            var dto = _mapper.Map<SellerDetailDto>(seller);

            // Add statistics
            dto.TotalOrders = await _unitOfWork.Orders.CountAsync(o => o.SellerId == sellerId && o.DeletedAt == null);
            dto.TotalSales = await _unitOfWork.Orders
                .SelectAll(o => o.SellerId == sellerId && o.DeletedAt == null)
                .SumAsync(o => o.FinalPrice);

            return Result<SellerDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<SellerDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PaginationResponse<SellerListDto>>> GetSellersAsync(SellerFilterRequest request)
    {
        try
        {
            var query = _unitOfWork.Sellers.Table.Where(s => s.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(s =>
                    s.FirstName.ToLower().Contains(term) ||
                    s.LastName.ToLower().Contains(term) ||
                    s.Phone.Contains(term));
            }

            if (request.IsActive.HasValue)
                query = query.Where(s => s.IsActive == request.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(request.Role))
                query = query.Where(s => s.Role == request.Role);

            query = query.OrderByDescending(s => s.CreatedAt);

            var total = await query.CountAsync();
            var sellers = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<SellerListDto>>(sellers);

            foreach (var dto in dtos)
            {
                dto.TotalOrders = await _unitOfWork.Orders.CountAsync(o => o.SellerId == dto.Id && o.DeletedAt == null);
            }

            return Result<PaginationResponse<SellerListDto>>.Success(
                new PaginationResponse<SellerListDto>(dtos, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex)
        {
            return Result<PaginationResponse<SellerListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SellerDetailDto>> CreateSellerAsync(CreateSellerDto request, int createdBy)
    {
        try
        {
            var existingPhone = await _unitOfWork.Sellers
                .AnyAsync(s => s.Phone == request.Phone && s.DeletedAt == null);

            if (existingPhone)
                return Result<SellerDetailDto>.Conflict("Phone number already exists");

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingEmail = await _unitOfWork.Sellers
                    .AnyAsync(s => s.Phone == request.Phone && s.DeletedAt == null);

                if (existingEmail)
                    return Result<SellerDetailDto>.Conflict("Email already exists");
            }

            var seller = _mapper.Map<Seller>(request);
            seller.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            seller.Role = request.Role ?? "Seller";
           // seller.MaxDiscountPercentage = request.Role == "Manager" ? 100 : 20;
            seller.IsActive = true;
            seller.CreatedBy = createdBy;

            await _unitOfWork.Sellers.InsertAsync(seller);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<SellerDetailDto>(seller);
            return Result<SellerDetailDto>.Success(dto, "Seller created successfully", 201);
        }
        catch (Exception ex)
        {
            return Result<SellerDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SellerDetailDto>> UpdateSellerAsync(int id, UpdateSellerDto request, int updatedBy)
    {
        try
        {
            var seller = await _unitOfWork.Sellers.GetByIdAsync(id);
            if (seller == null)
                return Result<SellerDetailDto>.NotFound("Seller not found");

            var phoneExists = await _unitOfWork.Sellers
                .AnyAsync(s => s.Phone == request.Phone && s.Id != id && s.DeletedAt == null);

            if (phoneExists)
                return Result<SellerDetailDto>.Conflict("Phone number already exists");

            seller.FirstName = request.FirstName;
            seller.LastName = request.LastName;
            seller.Phone = request.Phone;
            seller.Role = request.Role;
            //seller.MaxDiscountPercentage = request.Role == "Manager" ? 100 : 20;
            seller.UpdatedBy = updatedBy;
            seller.MarkAsUpdated();

            await _unitOfWork.Sellers.Update(seller, id);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<SellerDetailDto>(seller);
            return Result<SellerDetailDto>.Success(dto, "Seller updated successfully");
        }
        catch (Exception ex)
        {
            return Result<SellerDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteSellerAsync(int sellerId, int deletedBy)
    {
        try
        {
            var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
            if (seller == null)
                return Result.NotFound("Seller not found");

            seller.DeletedBy = deletedBy;
            seller.Delete();

            await _unitOfWork.Sellers.Update(seller, sellerId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Seller deleted successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SellerPerformanceDto>> GetSellerPerformanceAsync(int sellerId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
            if (seller == null)
                return Result<SellerPerformanceDto>.NotFound("Seller not found");

            var query = _unitOfWork.Orders.Table
                .Where(o => o.SellerId == sellerId && o.DeletedAt == null);

            if (startDate.HasValue)
                query = query.Where(o => DateTime.Parse(o.OrderDate) >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => DateTime.Parse(o.OrderDate) <= endDate.Value);

            var orders = await query.ToListAsync();

            var performance = new SellerPerformanceDto
            {
                SellerId = sellerId,
                SellerName = $"{seller.FirstName} {seller.LastName}",
                TotalOrders = orders.Count,
                TotalSales = orders.Sum(o => o.FinalPrice),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalPrice) : 0,
                TotalDiscountGiven = orders.Sum(o => o.DiscountApplied)
            };

            return Result<SellerPerformanceDto>.Success(performance);
        }
        catch (Exception ex)
        {
            return Result<SellerPerformanceDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TopSellerDto>>> GetTopSellersAsync(int count = 10)
    {
        try
        {
            var sellerOrders = await _unitOfWork.Orders
                .SelectAll(o => o.SellerId != null && o.DeletedAt == null, new[] { "Seller" })
                .GroupBy(o => o.SellerId)
                .Select(g => new TopSellerDto
                {
                    SellerId = g.Key.Value,
                    SellerName = $"{g.First().Seller.FirstName} {g.First().Seller.LastName}",
                    TotalOrders = g.Count(),
                    TotalSales = g.Sum(o => o.FinalPrice)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(count)
                .ToListAsync();

            return Result<List<TopSellerDto>>.Success(sellerOrders);
        }
        catch (Exception ex)
        {
            return Result<List<TopSellerDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ToggleSellerStatusAsync(int sellerId, int updatedBy)
    {
        try
        {
            var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
            if (seller == null)
                return Result.NotFound("Seller not found");

            seller.IsActive = !seller.IsActive;
            seller.UpdatedBy = updatedBy;
            seller.MarkAsUpdated();

            await _unitOfWork.Sellers.Update(seller, sellerId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"Seller {(seller.IsActive ? "activated" : "deactivated")}");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ChangeSellerRoleAsync(int sellerId, string newRole, int updatedBy)
    {
        try
        {
            var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
            if (seller == null)
                return Result.NotFound("Sotuvchi topilmadi");

            // Validate role using entity method
            try
            {
                seller.ChangeRole(newRole);
            }
            catch (ArgumentException ex)
            {
                return Result.BadRequest(ex.Message);
            }

            seller.UpdatedBy = updatedBy;
            seller.MarkAsUpdated();

            await _unitOfWork.Sellers.Update(seller, sellerId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"Sotuvchi roli '{newRole}' ga o'zgartirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<SellerListDto>>> SearchSellersAsync(string searchTerm)
    {
        try
        {
            var term = searchTerm.ToLower();
            var sellers = await _unitOfWork.Sellers.Table
                .Where(s => s.DeletedAt == null &&
                    (s.FirstName.ToLower().Contains(term) ||
                     s.LastName.ToLower().Contains(term) ||
                     s.Phone.Contains(term)))
                .Take(20)
                .ToListAsync();

            var dtos = _mapper.Map<List<SellerListDto>>(sellers);
            return Result<List<SellerListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<SellerListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAllSellersAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _unitOfWork.Sellers.Table.Where(s => s.DeletedAt == null);

            if (startDate.HasValue)
                query = query.Where(s => DateTime.Parse(s.CreatedAt) >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => DateTime.Parse(s.CreatedAt) <= endDate.Value);

            var sellers = await query.ToListAsync();

            foreach (var seller in sellers)
            {
                seller.DeletedBy = deletedBy;
                seller.Delete();
                await _unitOfWork.Sellers.Update(seller, seller.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success($"{sellers.Count} sellers deleted");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<SellerDetailDto>>> SeedMockSellersAsync(int createdBy, int count = 10)
    {
        try
        {
            var random = new Random();
            var sellers = new List<Seller>();

            var firstNames = new[] { "Ahmad", "Bobur", "Davron", "Eldor", "Farruh", "Gulnara", "Hilola", "Iroda", "Javlon", "Kamila" };
            var lastNames = new[] { "Azimov", "Boboev", "Dadaev", "Ergashev", "Fayziev", "Gulamov", "Hasanov", "Ibragimov", "Jalolov", "Karimov" };

            for (int i = 0; i < count; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var role = random.Next(0, 5) == 0 ? "Manager" : "Seller";

                var seller = new Seller
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = $"+998{random.Next(90, 99)}{random.Next(1000000, 9999999)}",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = role,
                    //MaxDiscountPercentage = role == "Manager" ? 100 : 20,
                    IsActive = true,
                    CreatedBy = createdBy
                };

                await _unitOfWork.Sellers.InsertAsync(seller);
                sellers.Add(seller);
            }

            await _unitOfWork.SaveChangesAsync();

            var dtos = _mapper.Map<List<SellerDetailDto>>(sellers);
            return Result<List<SellerDetailDto>>.Success(dtos, $"{count} mock sellers created");
        }
        catch (Exception ex)
        {
            return Result<List<SellerDetailDto>>.InternalError($"Error: {ex.Message}");
        }
    }
}
