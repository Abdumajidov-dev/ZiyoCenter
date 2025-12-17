using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICashbackService _cashbackService;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ICashbackService cashbackService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cashbackService = cashbackService;
    }

    public async Task<Result<OrderDetailDto>> GetOrderByIdAsync(int orderId, int userId, string userType)
    {
        try
        {
            var order = await _unitOfWork.Orders
                .SelectAsync(o => o.Id == orderId && o.DeletedAt == null,
                    new[] { "Customer", "Seller", "OrderItems", "OrderItems.Product" });

            if (order == null)
                return Result<OrderDetailDto>.NotFound("Order not found");

            // Authorization check
            if (userType == "Customer" && order.CustomerId != userId)
                return Result<OrderDetailDto>.Forbidden("Access denied");

            if (userType == "Seller" && order.SellerId != userId)
                return Result<OrderDetailDto>.Forbidden("Access denied");

            var dto = _mapper.Map<OrderDetailDto>(order);
            return Result<OrderDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OrderDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PaginationResponse<OrderListDto>>> GetOrdersAsync(
        OrderFilterRequest request, int userId, string userType)
    {
        try
        {
            var query = _unitOfWork.Orders.Table
                .Include(o => o.Customer)
                .Include(o => o.Seller)
                .Where(o => o.DeletedAt == null);

            // Filter by user type
            if (userType == "Customer")
                query = query.Where(o => o.CustomerId == userId);
            else if (userType == "Seller")
                query = query.Where(o => o.SellerId == userId);

            // Status filter
            if (request.Status.HasValue)
                query = query.Where(o => o.Status == request.Status.Value);

            // Date range
            if (request.DateFrom.HasValue)
                query = query.Where(o => DateTime.Parse(o.OrderDate) >= request.DateFrom.Value);

            if (request.DateTo.HasValue)
                query = query.Where(o => DateTime.Parse(o.OrderDate) <= request.DateTo.Value);

            query = query.OrderByDescending(o => o.OrderDate);

            var total = await query.CountAsync();
            var orders = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<OrderListDto>>(orders);

            return Result<PaginationResponse<OrderListDto>>.Success(
                new PaginationResponse<OrderListDto>(dtos, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex)
        {
            return Result<PaginationResponse<OrderListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<OrderDetailDto>> CreateOrderAsync(CreateOrderDto request, int customerId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null || !customer.IsActive)
                return Result<OrderDetailDto>.BadRequest("Invalid customer");

            // Get cart items or items from request
            List<OrderItem> orderItems;
            if (request.CreateFromCart)
            {
                var cartItems = await _unitOfWork.CartItems
                    .SelectAll(c => c.CustomerId == customerId && c.DeletedAt == null, new[] { "Product" })
                    .ToListAsync();

                if (!cartItems.Any())
                    return Result<OrderDetailDto>.BadRequest("Cart is empty");

                orderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price,
                }).ToList();
            }
            else
            {
                orderItems = new List<OrderItem>();
                foreach (var item in request.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product == null || !product.IsActive)
                        continue;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                    });
                }
            }

            if (!orderItems.Any())
                return Result<OrderDetailDto>.BadRequest("No valid items");

            // Validate stock
            foreach (var item in orderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product.StockQuantity < item.Quantity)
                    return Result<OrderDetailDto>.BadRequest($"Insufficient stock for {item.ProductName}");
            }

            // Calculate totals
            var totalPrice = orderItems.Sum(i => i.TotalPrice);

            // Validate cashback
            if (request.CashbackToUse > 0)
            {
                if (request.CashbackToUse > customer.CashbackBalance)
                    return Result<OrderDetailDto>.BadRequest("Insufficient cashback");

                if (request.CashbackToUse > totalPrice)
                    return Result<OrderDetailDto>.BadRequest("Cashback exceeds order total");
            }

            // Create order
            var order = new Order
            {
                OrderNumber = Order.GenerateOrderNumber(),
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = OrderStatus.Pending,
                TotalPrice = totalPrice,
                CashbackUsed = request.CashbackToUse,
                FinalPrice = totalPrice - request.CashbackToUse,
                PaymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod),
                DeliveryType = Enum.Parse<DeliveryType>(request.DeliveryType),
                DeliveryAddress = request.DeliveryAddress,
                CustomerNotes = request.CustomerNotes,
                CreatedBy = customerId
            };

            await _unitOfWork.Orders.InsertAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Add order items
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                await _unitOfWork.OrderItems.InsertAsync(item);

                // Decrease stock
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                product.DecreaseStock(item.Quantity);
                await _unitOfWork.Products.Update(product, product.Id);
            }

            // Use cashback
            if (request.CashbackToUse > 0)
            {
                await _cashbackService.UseCashbackAsync(customerId, order.Id, request.CashbackToUse);
            }

            // Clear cart
            if (request.CreateFromCart)
            {
                var cartItems = await _unitOfWork.CartItems
                    .SelectAll(c => c.CustomerId == customerId && c.DeletedAt == null)
                    .ToListAsync();

                foreach (var item in cartItems)
                {
                    item.Delete();
                    await _unitOfWork.CartItems.Update(item, item.Id);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var dto = await GetOrderDetailAsync(order.Id);
            return Result<OrderDetailDto>.Success(dto, "Order created successfully", 201);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<OrderDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<OrderDetailDto>> CreateOrderBySellerAsync(CreateOrderDto request, int sellerId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
            if (seller == null || !seller.IsActive)
                return Result<OrderDetailDto>.BadRequest("Invalid seller");

            var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return Result<OrderDetailDto>.BadRequest("Invalid customer");

            var orderItems = new List<OrderItem>();
            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsActive)
                    continue;

                if (product.StockQuantity < item.Quantity)
                    return Result<OrderDetailDto>.BadRequest($"Insufficient stock for {product.Name}");

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                });
            }

            if (!orderItems.Any())
                return Result<OrderDetailDto>.BadRequest("No valid items");

            var totalPrice = orderItems.Sum(i => i.TotalPrice);

            var order = new Order
            {
                OrderNumber = Order.GenerateOrderNumber(),
                CustomerId = request.CustomerId,
                SellerId = sellerId,
                OrderDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Status = OrderStatus.Confirmed,
                TotalPrice = totalPrice,
                DiscountApplied = request.DiscountAmount,
                FinalPrice = totalPrice - request.DiscountAmount,
                PaymentMethod = PaymentMethod.Cash,
                DeliveryType = DeliveryType.Pickup,
                SellerNotes = request.SellerNotes,
                CreatedBy = sellerId
            };

            await _unitOfWork.Orders.InsertAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Add items and decrease stock
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                await _unitOfWork.OrderItems.InsertAsync(item);

                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                product.DecreaseStock(item.Quantity);
                await _unitOfWork.Products.Update(product, product.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var dto = await GetOrderDetailAsync(order.Id);
            return Result<OrderDetailDto>.Success(dto, "Order created successfully", 201);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<OrderDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> CancelOrderAsync(int orderId, int userId, string userType, string? reason)
    {
        try
        {
            var order = await _unitOfWork.Orders
                .SelectAsync(o => o.Id == orderId && o.DeletedAt == null, new[] { "OrderItems" });

            if (order == null)
                return Result.NotFound("Order not found");

            // Authorization
            if (userType == "Customer" && order.CustomerId != userId)
                return Result.Forbidden("Access denied");

            // Can only cancel pending/confirmed orders
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                return Result.BadRequest("Cannot cancel order in current status");

            order.Status = OrderStatus.Cancelled;
            order.UpdatedBy = userId;
            order.MarkAsUpdated();

            // Restore stock
            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                product.AddStock(item.Quantity);
                await _unitOfWork.Products.Update(product, product.Id);
            }

            // Refund cashback if used
            if (order.CashbackUsed > 0)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerId);
                customer.AddCashback(order.CashbackUsed);
                await _unitOfWork.Customers.Update(customer, customer.Id);
            }

            await _unitOfWork.Orders.Update(order, orderId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Order cancelled successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> UpdateOrderStatusAsync(int orderId, string status, int updatedBy)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return Result.NotFound("Order not found");

            order.Status = Enum.Parse<OrderStatus>(status);
            order.UpdatedBy = updatedBy;
            order.MarkAsUpdated();

            // If delivered, earn cashback
            if (order.Status == OrderStatus.Delivered && order.PaidAt != null)
            {
                var cashbackAmount = order.FinalPrice * 0.02m;
                await _cashbackService.EarnCashbackAsync(order.CustomerId, order.Id, cashbackAmount);
            }

            await _unitOfWork.Orders.Update(order, orderId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Order status updated");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ConfirmOrderAsync(int orderId, int sellerId)
    {
        return await UpdateOrderStatusAsync(orderId, "Confirmed", sellerId);
    }

    public async Task<Result> MarkAsReadyForPickupAsync(int orderId, int updatedBy)
    {
        return await UpdateOrderStatusAsync(orderId, "ReadyForPickup", updatedBy);
    }

    public async Task<Result> MarkAsShippedAsync(int orderId, int updatedBy)
    {
        return await UpdateOrderStatusAsync(orderId, "Shipped", updatedBy);
    }

    public async Task<Result> MarkAsDeliveredAsync(int orderId, int updatedBy)
    {
        return await UpdateOrderStatusAsync(orderId, "Delivered", updatedBy);
    }

    public async Task<Result> ApplyDiscountAsync(ApplyDiscountDto request, int sellerId)
    {
        try
        {
            var order = await _unitOfWork.Orders
                .SelectAsync(o => o.Id == request.OrderId && o.DeletedAt == null, new[] { "OrderItems" });

            if (order == null)
                return Result.NotFound("Order not found");

            if (request.DiscountAmount > order.TotalPrice)
                return Result.BadRequest("Discount exceeds order total");

            var seller = await _unitOfWork.Sellers.GetByIdAsync(sellerId);
            if (seller == null)
                return Result.BadRequest("Invalid seller");

            // Check discount limit
            if (seller.Role != "Manager" && request.DiscountAmount > order.TotalPrice * 0.20m)
                return Result.BadRequest("Discount exceeds 20% limit");

            var orderDiscount = new OrderDiscount
            {
                OrderId = request.OrderId,
                DiscountReasonId = request.DiscountReasonId,
                Amount = request.DiscountAmount,
                AppliedBy = sellerId,
                Notes = request.Notes
            };

            await _unitOfWork.OrderDiscounts.InsertAsync(orderDiscount);

            order.DiscountApplied += request.DiscountAmount;
            order.FinalPrice = order.TotalPrice - order.DiscountApplied - order.CashbackUsed;
            order.UpdatedBy = sellerId;
            order.MarkAsUpdated();

            await _unitOfWork.Orders.Update(order, order.Id);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Discount applied successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> RemoveDiscountAsync(int orderDiscountId, int sellerId)
    {
        try
        {
            var discount = await _unitOfWork.OrderDiscounts
                .SelectAsync(d => d.Id == orderDiscountId && d.DeletedAt == null, new[] { "Order" });

            if (discount == null)
                return Result.NotFound("Discount not found");

            var order = discount.Order;
            order.DiscountApplied -= discount.Amount;
            order.FinalPrice = order.TotalPrice - order.DiscountApplied - order.CashbackUsed;
            order.UpdatedBy = sellerId;
            order.MarkAsUpdated();

            discount.Delete();
            await _unitOfWork.OrderDiscounts.Update(discount, orderDiscountId);
            await _unitOfWork.Orders.Update(order, order.Id);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Discount removed");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ProcessPaymentAsync(int orderId, string paymentMethod, string? reference)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return Result.NotFound("Order not found");

            order.PaymentMethod = Enum.Parse<PaymentMethod>(paymentMethod);
            order.PaymentReference = reference;
            order.PaidAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            order.MarkAsUpdated();

            await _unitOfWork.Orders.Update(order, orderId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Payment processed");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<OrderSummaryDto>> GetOrderSummaryAsync(DateTime dateFrom, DateTime dateTo)
    {
        try
        {
            var orders = await _unitOfWork.Orders
                .SelectAll(o => o.DeletedAt == null &&
                    DateTime.Parse(o.OrderDate) >= dateFrom &&
                    DateTime.Parse(o.OrderDate) <= dateTo)
                .ToListAsync();

            var summary = new OrderSummaryDto
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.FinalPrice),
                OnlineOrders = orders.Count(o => o.SellerId == null),
                OfflineOrders = orders.Count(o => o.SellerId != null),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                //CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalPrice) : 0
            };

            return Result<OrderSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            return Result<OrderSummaryDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<OrderListDto>>> GetCustomerOrdersAsync(int customerId)
    {
        try
        {
            var orders = await _unitOfWork.Orders
                .SelectAll(o => o.CustomerId == customerId && o.DeletedAt == null,
                    new[] { "Customer", "Seller" })
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<OrderListDto>>(orders);
            return Result<List<OrderListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<OrderListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<OrderListDto>>> GetSellerOrdersAsync(int sellerId)
    {
        try
        {
            var orders = await _unitOfWork.Orders
                .SelectAll(o => o.SellerId == sellerId && o.DeletedAt == null,
                    new[] { "Customer", "Seller" })
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<OrderListDto>>(orders);
            return Result<List<OrderListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<OrderListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAllOrdersAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _unitOfWork.Orders.Table.Where(o => o.DeletedAt == null);

            if (startDate.HasValue)
                query = query.Where(o => DateTime.Parse(o.OrderDate) >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => DateTime.Parse(o.OrderDate) <= endDate.Value);

            var orders = await query.ToListAsync();

            foreach (var order in orders)
            {
                order.DeletedBy = deletedBy;
                order.Delete();
                await _unitOfWork.Orders.Update(order, order.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success($"{orders.Count} orders deleted");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<OrderDetailDto>>> SeedMockOrdersAsync(
        int createdBy, int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var random = new Random();
            var customers = await _unitOfWork.Customers
                .SelectAll(c => c.DeletedAt == null && c.IsActive)
                .ToListAsync();

            var products = await _unitOfWork.Products
                .SelectAll(p => p.DeletedAt == null && p.IsActive && p.StockQuantity > 0)
                .ToListAsync();

            if (!customers.Any() || !products.Any())
                return Result<List<OrderDetailDto>>.BadRequest("No customers or products available");

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var orders = new List<Order>();

            for (int i = 0; i < count; i++)
            {
                var customer = customers[random.Next(customers.Count)];
                var orderDate = start.AddDays(random.Next((end - start).Days));

                var itemCount = random.Next(1, 5);
                var totalPrice = 0m;

                var order = new Order
                {
                    CustomerId = customer.Id,
                    OrderDate = orderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = (OrderStatus)random.Next(0, 7),
                    PaymentMethod = (PaymentMethod)random.Next(0, 4),
                    DeliveryType = (DeliveryType)random.Next(0, 3),
                    CreatedBy = createdBy
                };

                await _unitOfWork.Orders.InsertAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // Add order items
                for (int j = 0; j < itemCount; j++)
                {
                    var product = products[random.Next(products.Count)];
                    var quantity = random.Next(1, 3);

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        //TotalPrice = product.Price * quantity
                    };

                    await _unitOfWork.OrderItems.InsertAsync(orderItem);
                    totalPrice += orderItem.TotalPrice;
                }

                order.TotalPrice = totalPrice;
                order.FinalPrice = totalPrice;
                await _unitOfWork.Orders.Update(order, order.Id);

                orders.Add(order);
            }

            await _unitOfWork.SaveChangesAsync();

            var dtos = new List<OrderDetailDto>();
            foreach (var order in orders)
            {
                var dto = await GetOrderDetailAsync(order.Id);
                dtos.Add(dto);
            }

            return Result<List<OrderDetailDto>>.Success(dtos, $"{count} mock orders created");
        }
        catch (Exception ex)
        {
            return Result<List<OrderDetailDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    private async Task<OrderDetailDto> GetOrderDetailAsync(int orderId)
    {
        var order = await _unitOfWork.Orders
            .SelectAsync(o => o.Id == orderId,
                new[] { "Customer", "Seller", "OrderItems", "OrderItems.Product" });

        return _mapper.Map<OrderDetailDto>(order);
    }
}

