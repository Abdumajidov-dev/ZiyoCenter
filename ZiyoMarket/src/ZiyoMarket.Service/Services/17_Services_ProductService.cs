using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ===================== CRUD =====================

        public async Task<Result<ProductDetailDto>> GetProductByIdAsync(int productId, int? customerId = null)
        {
            var product = await _unitOfWork.Products
                .SelectAsync(p => p.Id == productId && p.DeletedAt == null, new[] { "Category" });

            if (product == null)
                return Result<ProductDetailDto>.NotFound($"Product with ID {productId} not found");

            var dto = _mapper.Map<ProductDetailDto>(product);

            dto.LikesCount = await _unitOfWork.ProductLikes.CountAsync(l => l.ProductId == productId && l.DeletedAt == null);

            if (customerId.HasValue)
                dto.IsLikedByCurrentUser = await _unitOfWork.ProductLikes
                    .AnyAsync(l => l.ProductId == productId && l.CustomerId == customerId.Value && l.DeletedAt == null);

            return Result<ProductDetailDto>.Success(dto);
        }

        public async Task<Result<ProductDetailDto>> GetProductByQRCodeAsync(string qrCode, int? customerId = null)
        {
            var product = await _unitOfWork.Products
                .SelectAsync(p => p.QrCode == qrCode && p.DeletedAt == null, new[] { "Category" });

            if (product == null)
                return Result<ProductDetailDto>.NotFound($"Product with QR code '{qrCode}' not found");

            var dto = _mapper.Map<ProductDetailDto>(product);
            dto.LikesCount = await _unitOfWork.ProductLikes.CountAsync(l => l.ProductId == product.Id && l.DeletedAt == null);

            if (customerId.HasValue)
                dto.IsLikedByCurrentUser = await _unitOfWork.ProductLikes
                    .AnyAsync(l => l.ProductId == product.Id && l.CustomerId == customerId.Value && l.DeletedAt == null);

            return Result<ProductDetailDto>.Success(dto);
        }

        public async Task<Result<PaginationResponse<ProductListDto>>> GetProductsAsync(ProductFilterRequest request, int? customerId = null)
        {
            var query = _unitOfWork.Products.SelectAll(p => p.DeletedAt == null, new[] { "Category" });

            // Filtering
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
            }

            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice.Value);

            if (request.InStock == true)
                query = query.Where(p => p.StockQuantity > 0);

            if (request.LowStock == true)
                query = query.Where(p => p.StockQuantity <= p.MinStockLevel && p.StockQuantity > 0);

            query = query.OrderByDescending(p => p.CreatedAt);

            var total = await query.CountAsync();

            var products = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync();

            var resultDtos = _mapper.Map<List<ProductListDto>>(products);

            if (customerId.HasValue)
            {
                var likedIds = await _unitOfWork.ProductLikes
                    .SelectAll(l => l.CustomerId == customerId.Value && l.DeletedAt == null)
                    .Select(l => l.ProductId)
                    .ToListAsync();

                foreach (var dto in resultDtos)
                    dto.IsLikedByCurrentUser = likedIds.Contains(dto.Id);
            }

            var likes = await _unitOfWork.ProductLikes
                .SelectAll(l => l.DeletedAt == null)
                .GroupBy(l => l.ProductId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var dto in resultDtos)
                dto.LikesCount = likes.FirstOrDefault(l => l.Key == dto.Id)?.Count ?? 0;

            return Result<PaginationResponse<ProductListDto>>.Success(
                new PaginationResponse<ProductListDto>(resultDtos, total, request.PageNumber, request.PageSize)
            );
        }

        public async Task<Result<ProductDetailDto>> CreateProductAsync(CreateProductDto request, int createdBy)
        {
            var exists = await _unitOfWork.Products.AnyAsync(p => p.QrCode == request.QRCode && p.DeletedAt == null);
            if (exists)
                return Result<ProductDetailDto>.Conflict("Product with this QR code already exists");

            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
            if (category == null || !category.IsActive)
                return Result<ProductDetailDto>.BadRequest("Invalid or inactive category");

            var product = _mapper.Map<Product>(request);
            product.CreatedBy = createdBy;
            product.Status = ProductStatus.Active;
            product.SearchText = $"{product.Name} {product.Description}".ToLower();

            await _unitOfWork.Products.InsertAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return Result<ProductDetailDto>.Success(_mapper.Map<ProductDetailDto>(product), "Product created successfully", 201);
        }

        public async Task<Result<ProductDetailDto>> UpdateProductAsync(UpdateProductDto request, int updatedBy)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
            if (product == null)
                return Result<ProductDetailDto>.NotFound("Product not found");

            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
            if (category == null || !category.IsActive)
                return Result<ProductDetailDto>.BadRequest("Invalid or inactive category");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;
            product.ImageUrl = request.ImageUrl;
            product.MinStockLevel = request.MinStockLevel;
            product.UpdatedBy = updatedBy;
            product.MarkAsUpdated();

            await _unitOfWork.Products.Update(product, product.Id);
            await _unitOfWork.SaveChangesAsync();

            return Result<ProductDetailDto>.Success(_mapper.Map<ProductDetailDto>(product), "Product updated successfully");
        }

        public async Task<Result> DeleteProductAsync(int productId, int deletedBy)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return Result.NotFound("Product not found");

            product.DeletedBy = deletedBy;
            product.Delete();

            await _unitOfWork.Products.Update(product, product.Id);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Product deleted successfully");
        }

        // ===================== STOCK =====================

        public async Task<Result> UpdateStockAsync(UpdateStockDto request, int updatedBy)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (product == null)
                return Result.NotFound("Product not found");

            switch (request.Operation.ToLower())
            {
                case "add":
                    product.AddStock(request.Quantity);
                    break;
                case "remove":
                    product.DecreaseStock(request.Quantity);
                    break;
                case "set":
                    product.StockQuantity = request.Quantity;
                    break;
                default:
                    return Result.BadRequest("Invalid operation");
            }

            product.UpdatedBy = updatedBy;
            product.MarkAsUpdated();

            await _unitOfWork.Products.Update(product, product.Id);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Stock updated successfully");
        }

        public async Task<Result> AddStockAsync(int productId, int quantity, string? reason, int updatedBy)
            => await UpdateStockAsync(new UpdateStockDto { ProductId = productId, Quantity = quantity, Operation = "add", Reason = reason }, updatedBy);

        public async Task<Result> RemoveStockAsync(int productId, int quantity, string? reason, int updatedBy)
            => await UpdateStockAsync(new UpdateStockDto { ProductId = productId, Quantity = quantity, Operation = "remove", Reason = reason }, updatedBy);

        public async Task<Result<List<LowStockProductDto>>> GetLowStockProductsAsync()
        {
            var products = await _unitOfWork.Products
                .SelectAll(p => p.DeletedAt == null && p.StockQuantity <= p.MinStockLevel && p.StockQuantity > 0)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            return Result<List<LowStockProductDto>>.Success(_mapper.Map<List<LowStockProductDto>>(products));
        }

        // ===================== LIKES =====================

        public async Task<Result> ToggleLikeAsync(int productId, int customerId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return Result.NotFound("Product not found");

            var existing = await _unitOfWork.ProductLikes
                .SelectAsync(l => l.ProductId == productId && l.CustomerId == customerId && l.DeletedAt == null);

            if (existing != null)
            {
                existing.Delete();
                await _unitOfWork.ProductLikes.Update(existing, existing.Id);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success("Product unliked");
            }

            var newLike = new ProductLike
            {
                ProductId = productId,
                CustomerId = customerId,
                LikedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductLikes.InsertAsync(newLike);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Product liked");
        }

        public async Task<Result<List<ProductListDto>>> GetLikedProductsAsync(int customerId)
        {
            var likedIds = await _unitOfWork.ProductLikes
                .SelectAll(l => l.CustomerId == customerId && l.DeletedAt == null)
                .Select(l => l.ProductId)
                .ToListAsync();

            if (!likedIds.Any())
                return Result<List<ProductListDto>>.Success(new List<ProductListDto>());

            var products = await _unitOfWork.Products
                .SelectAll(p => likedIds.Contains(p.Id) && p.DeletedAt == null, new[] { "Category" })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProductListDto>>(products);
            foreach (var dto in dtos)
                dto.IsLikedByCurrentUser = true;

            return Result<List<ProductListDto>>.Success(dtos);
        }

        // ===================== SPECIAL =====================

        public async Task<Result<List<ProductListDto>>> GetFeaturedProductsAsync(int count = 10)
        {
            var likeCounts = await _unitOfWork.ProductLikes
                .SelectAll(l => l.DeletedAt == null)
                .GroupBy(l => l.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .ToListAsync();

            var ids = likeCounts.Select(x => x.ProductId).ToList();

            var products = await _unitOfWork.Products
                .SelectAll(p => ids.Contains(p.Id) && p.DeletedAt == null && p.IsActive, new[] { "Category" })
                .ToListAsync();

            var dtos = _mapper.Map<List<ProductListDto>>(products);

            foreach (var dto in dtos)
                dto.LikesCount = likeCounts.FirstOrDefault(l => l.ProductId == dto.Id)?.Count ?? 0;

            return Result<List<ProductListDto>>.Success(dtos.OrderByDescending(d => d.LikesCount).ToList());
        }

        public async Task<Result<List<ProductListDto>>> GetNewArrivalsAsync(int count = 10)
        {
            var products = await _unitOfWork.Products
                .SelectAll(p => p.DeletedAt == null && p.IsActive, new[] { "Category" })
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            return Result<List<ProductListDto>>.Success(_mapper.Map<List<ProductListDto>>(products));
        }

        public async Task<Result<List<ProductListDto>>> SearchProductsAsync(
    string searchTerm, int? categoryId = null, int? customerId = null)
        {
            try
            {
                var query = _unitOfWork.Products.Table
                            .Include(p => p.Category)
                            .Where(p => p.DeletedAt == null && p.IsActive);


                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearch = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(lowerSearch) ||
                        p.Description.ToLower().Contains(lowerSearch) ||
                        p.QrCode.ToLower().Contains(lowerSearch));
                }

                if (categoryId.HasValue)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(50)
                    .ToListAsync();

                var dtos = _mapper.Map<List<ProductListDto>>(products);

                // Like ma'lumotlarini qo�shish
                if (customerId.HasValue && dtos.Any())
                {
                    var productIds = products.Select(p => p.Id).ToList();

                    var likedIds = await _unitOfWork.ProductLikes
                        .SelectAll(l => l.CustomerId == customerId.Value &&
                                    productIds.Contains(l.ProductId) &&
                                    l.DeletedAt == null)
                        .Select(l => l.ProductId)
                        .ToListAsync();

                    foreach (var dto in dtos)
                    {
                        dto.IsLikedByCurrentUser = likedIds.Contains(dto.Id);
                    }
                }

                return Result<List<ProductListDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ProductListDto>>.InternalError($"Error searching products: {ex.Message}");
            }
        }


        public async Task<Result<List<ProductListDto>>> GetProductsByCategoryAsync(
            int categoryId, int? customerId = null)
        {
            try
            {
                var query = _unitOfWork.Products.Table
                            .Include(p => p.Category)
                            .Where(p => p.DeletedAt == null && p.IsActive);


                var dtos = _mapper.Map<List<ProductListDto>>(query);

                // Liked mahsulotlarni belgilash
                if (customerId.HasValue && dtos.Any())
                {
                    var productIds = query.Select(p => p.Id).ToList();

                    var likedIds = await _unitOfWork.ProductLikes
                        .SelectAll(l => l.CustomerId == customerId.Value &&
                                    productIds.Contains(l.ProductId) &&
                                    l.DeletedAt == null)
                        .Select(l => l.ProductId)
                        .ToListAsync();

                    foreach (var dto in dtos)
                    {
                        dto.IsLikedByCurrentUser = likedIds.Contains(dto.Id);
                    }
                }

                return Result<List<ProductListDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<ProductListDto>>.InternalError($"Error retrieving products by category: {ex.Message}");
            }
        }

    }
}
