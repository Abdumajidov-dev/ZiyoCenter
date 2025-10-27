using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.Extensions;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Category management service implementation
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = _mapper;
    }

    #region CRUD Operations

    public async Task<Result<CategoryDto>> GetCategoryByIdAsync(int categoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == categoryId && !c.IsDeleted,
                    new[] { "Parent", "Children", "Products" });

            if (category == null)
                return Result<CategoryDto>.NotFound("Kategoriya topilmadi");

            var dto = _mapper.Map<CategoryDto>(category);
            dto.ProductCount = category.Products.Count(p => p.IsActive && !p.IsDeleted);

            return Result<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<CategoryDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryDto>>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.Table
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            // Add product counts
            for (int i = 0; i < categories.Count; i++)
            {
                dtos[i].ProductCount = categories[i].Products.Count(p => p.IsActive && !p.IsDeleted);
            }

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryDto>>> GetRootCategoriesAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.Table
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted && c.ParentId == null)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            // Add product counts
            for (int i = 0; i < categories.Count; i++)
            {
                dtos[i].ProductCount = categories[i].Products.Count(p => p.IsActive && !p.IsDeleted);
            }

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryDto>>> GetSubCategoriesAsync(int parentId)
    {
        try
        {
            // Check if parent exists
            var parent = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == parentId && !c.IsDeleted);

            if (parent == null)
                return Result<List<CategoryDto>>.NotFound("Ota kategoriya topilmadi");

            var categories = await _unitOfWork.Categories.Table
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted && c.ParentId == parentId)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            // Add product counts
            for (int i = 0; i < categories.Count; i++)
            {
                dtos[i].ProductCount = categories[i].Products.Count(p => p.IsActive && !p.IsDeleted);
            }

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(SaveCategoryDto request, int createdBy)
    {
        try
        {
            // Validate parent if provided
            if (request.ParentId.HasValue)
            {
                var parent = await _unitOfWork.Categories
                    .SelectAsync(c => c.Id == request.ParentId.Value && !c.IsDeleted);

                if (parent == null)
                    return Result<CategoryDto>.NotFound("Ota kategoriya topilmadi");
            }

            // Check if category with same name exists
            var existingCategory = await _unitOfWork.Categories.Table
                .FirstOrDefaultAsync(c => c.Name == request.Name && !c.IsDeleted);

            if (existingCategory != null)
                return Result<CategoryDto>.BadRequest("Bu nom bilan kategoriya allaqachon mavjud");

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                ParentId = request.ParentId,
                DisplayOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreatedBy = createdBy
            };

           // category.MarkAsCreated();
            await _unitOfWork.Categories.InsertAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CategoryDto>(category);
            dto.ProductCount = 0;

            return Result<CategoryDto>.Success(dto, "Kategoriya muvaffaqiyatli yaratildi");
        }
        catch (Exception ex)
        {
            return Result<CategoryDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(int id, SaveCategoryDto request, int updatedBy)
    {
        try
        {
            var category = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == id && !c.IsDeleted, new[] { "Products" });

            if (category == null)
                return Result<CategoryDto>.NotFound("Kategoriya topilmadi");

            // Validate parent if provided
            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == id)
                    return Result<CategoryDto>.BadRequest("Kategoriya o'zini ota qilib qo'ya olmaydi");

                var parent = await _unitOfWork.Categories
                    .SelectAsync(c => c.Id == request.ParentId.Value && !c.IsDeleted);

                if (parent == null)
                    return Result<CategoryDto>.NotFound("Ota kategoriya topilmadi");

                // Check for circular reference
                if (await IsCircularReference(id, request.ParentId.Value))
                    return Result<CategoryDto>.BadRequest("Tsiklik havola yaratib bo'lmaydi");
            }

            // Check if name is unique (excluding current category)
            var existingCategory = await _unitOfWork.Categories.Table
                .FirstOrDefaultAsync(c => c.Name == request.Name && c.Id != id && !c.IsDeleted);

            if (existingCategory != null)
                return Result<CategoryDto>.BadRequest("Bu nom bilan kategoriya allaqachon mavjud");

            category.ChangeName(request.Name);
            category.UpdateDescription(request.Description);
            category.ChangeParent(request.ParentId);
            category.ChangeDisplayOrder(request.SortOrder);
            category.IsActive = request.IsActive;
            category.UpdatedBy = updatedBy;
            category.MarkAsUpdated();

            await _unitOfWork.Categories.Update(category,category.Id);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CategoryDto>(category);
            dto.ProductCount = category.Products.Count(p => p.IsActive && !p.IsDeleted);

            return Result<CategoryDto>.Success(dto, "Kategoriya muvaffaqiyatli yangilandi");
        }
        catch (Exception ex)
        {
            return Result<CategoryDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId, int deletedBy)
    {
        try
        {
            var category = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == categoryId && !c.IsDeleted,
                    new[] { "Products", "Children" });

            if (category == null)
                return Result.NotFound("Kategoriya topilmadi");

            // Check if category has products
            if (category.Products.Any(p => !p.IsDeleted))
                return Result.BadRequest("Bu kategoriyada mahsulotlar mavjud. Avval mahsulotlarni o'chiring yoki boshqa kategoriyaga o'tkazing");

            // Check if category has children
            if (category.Children.Any(c => !c.IsDeleted))
                return Result.BadRequest("Bu kategoriyada ichki kategoriyalar mavjud. Avval ichki kategoriyalarni o'chiring");

            category.DeletedBy = deletedBy;
            //category.MarkAsDeleted();
            _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Kategoriya muvaffaqiyatli o'chirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion

    #region Hierarchy Operations

    public async Task<Result<List<CategoryDto>>> GetCategoryTreeAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.Table
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            // Add product counts
            for (int i = 0; i < categories.Count; i++)
            {
                dtos[i].ProductCount = categories[i].Products.Count(p => p.IsActive && !p.IsDeleted);
            }

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<string>> GetCategoryPathAsync(int categoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == categoryId && !c.IsDeleted, new[] { "Parent" });

            if (category == null)
                return Result<string>.NotFound("Kategoriya topilmadi");

            var path = category.GetFullPath();
            return Result<string>.Success(path);
        }
        catch (Exception ex)
        {
            return Result<string>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryDto>>> GetCategoryWithChildrenAsync(int categoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == categoryId && !c.IsDeleted,
                    new[] { "Children", "Children.Products" });

            if (category == null)
                return Result<List<CategoryDto>>.NotFound("Kategoriya topilmadi");

            var children = category.Children.Where(c => !c.IsDeleted).ToList();
            var dtos = _mapper.Map<List<CategoryDto>>(children);

            // Add product counts
            for (int i = 0; i < children.Count; i++)
            {
                dtos[i].ProductCount = children[i].Products.Count(p => p.IsActive && !p.IsDeleted);
            }

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion

    #region Search & Filter

    public async Task<Result<List<CategoryDto>>> SearchCategoriesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCategoriesAsync();

            var categories = await _unitOfWork.Categories.Table
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted &&
                           (c.Name.Contains(searchTerm) ||
                            (c.Description != null && c.Description.Contains(searchTerm))))
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            // Add product counts
            for (int i = 0; i < categories.Count; i++)
            {
                dtos[i].ProductCount = categories[i].Products.Count(p => p.IsActive && !p.IsDeleted);
            }

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryDto>>> GetActiveCategoriesAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.Table
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            // Add product counts
            for (int i = 0; i < categories.Count; i++)
            {
                dtos[i].ProductCount = categories[i].Products.Count(p => p.IsActive && !p.IsDeleted);
            }

            return Result<List<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion

    #region Product Count

    public async Task<Result<CategoryWithProductCountDto>> GetCategoryWithProductCountAsync(int categoryId)
    {
        try
        {
            var category = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == categoryId && !c.IsDeleted,
                    new[] { "Parent", "Products" });

            if (category == null)
                return Result<CategoryWithProductCountDto>.NotFound("Kategoriya topilmadi");

            var dto = new CategoryWithProductCountDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                ParentId = category.ParentId,
                ParentName = category.Parent?.Name,
                ActiveProductCount = category.Products.Count(p => p.IsActive && !p.IsDeleted),
                TotalProductCount = category.Products.Count(p => !p.IsDeleted),
                IsActive = category.IsActive,
                UpdatedAt = category.UpdatedAt
            };

            return Result<CategoryWithProductCountDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<CategoryWithProductCountDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryWithProductCountDto>>> GetAllCategoriesWithProductCountAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.Table
                .Include(c => c.Parent)
                .Include(c => c.Products)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var dtos = categories.Select(c => new CategoryWithProductCountDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                ParentId = c.ParentId,
                ParentName = c.Parent?.Name,
                ActiveProductCount = c.Products.Count(p => p.IsActive && !p.IsDeleted),
                TotalProductCount = c.Products.Count(p => !p.IsDeleted),
                IsActive = c.IsActive,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return Result<List<CategoryWithProductCountDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CategoryWithProductCountDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion

    #region Admin Operations

    public async Task<Result> ReorderCategoriesAsync(List<ReorderCategoryDto> categories, int updatedBy)
    {
        try
        {
            foreach (var item in categories)
            {
                var category = await _unitOfWork.Categories
                    .SelectAsync(c => c.Id == item.Id && !c.IsDeleted);

                if (category != null)
                {
                    category.ChangeDisplayOrder(item.SortOrder);
                    category.UpdatedBy = updatedBy;
                    category.MarkAsUpdated();
                    _unitOfWork.Categories.Update(category,category.Id);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success("Kategoriyalar tartibi yangilandi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result> ToggleCategoryStatusAsync(int categoryId, int updatedBy)
    {
        try
        {
            var category = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == categoryId && !c.IsDeleted);

            if (category == null)
                return Result.NotFound("Kategoriya topilmadi");

            if (category.IsActive)
                category.Deactivate();
            else
                category.Activate();

            category.UpdatedBy = updatedBy;
            _unitOfWork.Categories.Update(category,category.Id);
            await _unitOfWork.SaveChangesAsync();

            var status = category.IsActive ? "faollashtirildi" : "faolsizlashtirildi";
            return Result.Success($"Kategoriya {status}");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<Result> DeleteAllCategoriesAsync(int deletedBy, string? startDate = null, string? endDate = null)
    {
        try
        {
            var query = _unitOfWork.Categories.Table
                .Include(c => c.Products)
                .Include(c => c.Children)
                .Where(c => !c.IsDeleted);

            if (startDate.IsNullOrEmpty())
                query = query.Where(c => c.CreatedAt == startDate);

            if (endDate.IsNullOrEmpty())
                query = query.Where(c => c.CreatedAt == endDate);

            var categories = await query.ToListAsync();

            // Only delete categories without products or children
            var deletableCategories = categories
                .Where(c => !c.Products.Any(p => !p.IsDeleted) &&
                           !c.Children.Any(child => !child.IsDeleted))
                .ToList();

            if (!deletableCategories.Any())
                return Result.Success("O'chirilishi mumkin bo'lgan kategoriyalar topilmadi");

            foreach (var category in deletableCategories)
            {
                category.DeletedBy = deletedBy;
                //category.MarkAsDeleted();
                _unitOfWork.Categories.Delete(category);
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"{deletableCategories.Count} ta kategoriya o'chirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<CategoryDto>>> SeedMockCategoriesAsync(int createdBy, int count = 10)
    {
        try
        {
            var mockCategories = new List<Category>();
            var random = new Random();

            // Create root categories
            var rootCategories = new[]
            {
                new Category { Name = "Elektronika", Description = "Elektronik qurilmalar", IsActive = true, DisplayOrder = 1, CreatedBy = createdBy },
                new Category { Name = "Kiyim", Description = "Erkaklar va ayollar kiyimlari", IsActive = true, DisplayOrder = 2, CreatedBy = createdBy },
                new Category { Name = "Kitoblar", Description = "Turli xil kitoblar", IsActive = true, DisplayOrder = 3, CreatedBy = createdBy },
                new Category { Name = "Oshxona", Description = "Oshxona jihozlari", IsActive = true, DisplayOrder = 4, CreatedBy = createdBy },
                new Category { Name = "Sport", Description = "Sport buyumlari", IsActive = true, DisplayOrder = 5, CreatedBy = createdBy }
            };

            foreach (var category in rootCategories)
            {
               // category.MarkAsCreated();
                await _unitOfWork.Categories.InsertAsync(category);
                mockCategories.Add(category);
            }

            await _unitOfWork.SaveChangesAsync();

            // Create subcategories
            var subCategories = new[]
            {
                new Category { Name = "Telefonlar", Description = "Smartfonlar va telefonlar", ParentId = rootCategories[0].Id, IsActive = true, DisplayOrder = 1, CreatedBy = createdBy },
                new Category { Name = "Noutbuklar", Description = "Noutbuk va planshetlar", ParentId = rootCategories[0].Id, IsActive = true, DisplayOrder = 2, CreatedBy = createdBy },
                new Category { Name = "Erkaklar kiyimi", Description = "Erkaklar uchun kiyimlar", ParentId = rootCategories[1].Id, IsActive = true, DisplayOrder = 1, CreatedBy = createdBy },
                new Category { Name = "Ayollar kiyimi", Description = "Ayollar uchun kiyimlar", ParentId = rootCategories[1].Id, IsActive = true, DisplayOrder = 2, CreatedBy = createdBy },
                new Category { Name = "Badiiy adabiyot", Description = "Romanlar va hikoyalar", ParentId = rootCategories[2].Id, IsActive = true, DisplayOrder = 1, CreatedBy = createdBy }
            };

            foreach (var category in subCategories)
            {
                if (mockCategories.Count >= count)
                    break;

               // category.MarkAsCreated();
                await _unitOfWork.Categories.InsertAsync(category);
                mockCategories.Add(category);
            }

            await _unitOfWork.SaveChangesAsync();

            var dtos = _mapper.Map<List<CategoryDto>>(mockCategories);
            return Result<List<CategoryDto>>.Success(dtos, $"{mockCategories.Count} ta mock kategoriya yaratildi");
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    private async Task<bool> IsCircularReference(int categoryId, int parentId)
    {
        var current = await _unitOfWork.Categories
            .SelectAsync(c => c.Id == parentId && !c.IsDeleted, new[] { "Parent" });

        while (current != null)
        {
            if (current.Id == categoryId)
                return true;

            if (!current.ParentId.HasValue)
                break;

            current = await _unitOfWork.Categories
                .SelectAsync(c => c.Id == current.ParentId.Value && !c.IsDeleted, new[] { "Parent" });
        }

        return false;
    }

    public Task<Result> DeleteAllCategoriesAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        throw new NotImplementedException();
    }

    #endregion
}
