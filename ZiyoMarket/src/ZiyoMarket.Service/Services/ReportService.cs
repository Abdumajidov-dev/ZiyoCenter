using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Service.DTOs.Reports;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReportService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
   _mapper = mapper;
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync(DateTime startDate, DateTime endDate)
    {
      try
     {
            var stats = new DashboardStatsDto();

            var orders = await _unitOfWork.Orders
 .SelectAll(o => !o.IsDeleted &&
        DateTime.Parse(o.OrderDate) >= startDate &&
           DateTime.Parse(o.OrderDate) <= endDate)
    .ToListAsync();

    stats.TotalOrders = orders.Count;
       stats.TotalRevenue = orders.Sum(o => o.FinalPrice);
        stats.TotalDiscounts = orders.Sum(o => o.DiscountApplied);
            stats.AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalPrice) : 0;

 stats.OnlineOrders = orders.Count(o => o.SellerId == null);
   stats.OfflineOrders = orders.Count(o => o.SellerId != null);
          stats.CancelledOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled);

            stats.CustomerCount = await _unitOfWork.Customers.CountAsync(c => !c.IsDeleted);
 stats.ProductCount = await _unitOfWork.Products.CountAsync(p => !p.IsDeleted);
            stats.LowStockProducts = await _unitOfWork.Products.CountAsync(p => !p.IsDeleted && p.IsLowStock);

            return Result<DashboardStatsDto>.Success(stats);
        }
        catch (Exception ex)
  {
            return Result<DashboardStatsDto>.InternalError($"Error: {ex.Message}");
        }
    }

  public async Task<Result<List<ChartDataDto>>> GetSalesChartDataAsync(
        DateTime startDate, DateTime endDate, string groupBy = "day")
    {
        try
        {
       var orders = await _unitOfWork.Orders
      .SelectAll(o => !o.IsDeleted &&
       DateTime.Parse(o.OrderDate) >= startDate &&
      DateTime.Parse(o.OrderDate) <= endDate)
  .ToListAsync();

         var chartData = new List<ChartDataDto>();

            switch (groupBy.ToLower())
  {
     case "day":
       chartData = orders
      .GroupBy(o => DateTime.Parse(o.OrderDate).Date)
        .Select(g => new ChartDataDto
           {
 Label = g.Key.ToString("dd/MM/yyyy"),
   Value = g.Sum(o => o.FinalPrice)
           })
   .OrderBy(x => DateTime.ParseExact(x.Label, "dd/MM/yyyy", null))
        .ToList();
              break;

        case "month":
            chartData = orders
            .GroupBy(o => new { month = DateTime.Parse(o.OrderDate).Month, year = DateTime.Parse(o.OrderDate).Year })
          .Select(g => new ChartDataDto
          {
         Label = $"{g.Key.month}/{g.Key.year}",
   Value = g.Sum(o => o.FinalPrice)
            })
        .OrderBy(x => DateTime.ParseExact(x.Label, "M/yyyy", null))
           .ToList();
    break;

            case "year":
       chartData = orders
  .GroupBy(o => DateTime.Parse(o.OrderDate).Year)
    .Select(g => new ChartDataDto
         {
              Label = g.Key.ToString(),
       Value = g.Sum(o => o.FinalPrice)
               })
 .OrderBy(x => x.Label)
         .ToList();
         break;
   }

            return Result<List<ChartDataDto>>.Success(chartData);
        }
        catch (Exception ex)
   {
      return Result<List<ChartDataDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<SalesReportDto>> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        try
      {
        var orders = await _unitOfWork.Orders
           .SelectAll(o => !o.IsDeleted &&
               DateTime.Parse(o.OrderDate) >= startDate &&
            DateTime.Parse(o.OrderDate) <= endDate)
         .ToListAsync();

            var report = new SalesReportDto
      {
   StartDate = startDate,
     EndDate = endDate,
    TotalOrders = orders.Count,
      TotalRevenue = orders.Sum(o => o.FinalPrice),
    TotalDiscount = orders.Sum(o => o.DiscountApplied),
                TotalCashbackUsed = orders.Sum(o => o.CashbackUsed),
   AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalPrice) : 0,
   OnlineOrders = orders.Count(o => o.SellerId == null),
              OfflineOrders = orders.Count(o => o.SellerId != null),
        CancelledOrders = orders.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled),
         OrdersByStatus = orders.GroupBy(o => o.Status)
         .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
   .ToDictionary(x => x.Status, x => x.Count)
     };

          return Result<SalesReportDto>.Success(report);
        }
        catch (Exception ex)
        {
   return Result<SalesReportDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<ProductSalesReportDto>> GetProductSalesReportAsync(DateTime startDate, DateTime endDate)
    {
     try
        {
     var orderItems = await _unitOfWork.OrderItems
                .SelectAll(oi => !oi.IsDeleted && 
        !oi.Order.IsDeleted &&
    DateTime.Parse(oi.Order.OrderDate) >= startDate &&
DateTime.Parse(oi.Order.OrderDate) <= endDate,
       new[] { "Order", "Product" })
             .ToListAsync();

 var report = new ProductSalesReportDto
       {
     StartDate = startDate,
                EndDate = endDate,
 TotalProductsSold = orderItems.Sum(oi => oi.Quantity),
     TotalRevenue = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
       AverageUnitPrice = orderItems.Any() ? orderItems.Average(oi => oi.UnitPrice) : 0,
       ProductSales = orderItems
   .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
               .Select(g => new ProductSaleDto
       {
    ProductId = g.Key.ProductId,
          ProductName = g.Key.Name,
     QuantitySold = g.Sum(oi => oi.Quantity),
         Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity),
  AveragePrice = g.Average(oi => oi.UnitPrice)
          })
     .OrderByDescending(x => x.Revenue)
     .ToList()
          };

       return Result<ProductSalesReportDto>.Success(report);
        }
        catch (Exception ex)
        {
  return Result<ProductSalesReportDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CategorySalesReportDto>> GetCategorySalesReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
   var orderItems = await _unitOfWork.OrderItems
      .SelectAll(oi => !oi.IsDeleted &&
   !oi.Order.IsDeleted &&
          DateTime.Parse(oi.Order.OrderDate) >= startDate &&
      DateTime.Parse(oi.Order.OrderDate) <= endDate,
            new[] { "Order", "Product", "Product.Category" })
          .ToListAsync();

            var report = new CategorySalesReportDto
            {
          StartDate = startDate,
                EndDate = endDate,
        CategorySales = orderItems
           .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
         .Select(g => new CategorySaleDto
    {
          CategoryId = g.Key.CategoryId,
             CategoryName = g.Key.Name,
        ProductsSold = g.Sum(oi => oi.Quantity),
       Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity),
          OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
        })
       .OrderByDescending(x => x.Revenue)
          .ToList()
  };

 return Result<CategorySalesReportDto>.Success(report);
        }
        catch (Exception ex)
  {
        return Result<CategorySalesReportDto>.InternalError($"Error: {ex.Message}");
 }
    }

    public async Task<Result<List<TopProductDto>>> GetTopProductsAsync(
        DateTime startDate, DateTime endDate, int count = 10)
    {
        try
        {
        var topProducts = await _unitOfWork.OrderItems
      .SelectAll(oi => !oi.IsDeleted &&
     !oi.Order.IsDeleted &&
     DateTime.Parse(oi.Order.OrderDate) >= startDate &&
           DateTime.Parse(oi.Order.OrderDate) <= endDate,
     new[] { "Order", "Product" })
              .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
     .Select(g => new TopProductDto
                {
        ProductId = g.Key.ProductId,
        ProductName = g.Key.Name,
               TotalSales = g.Sum(oi => oi.UnitPrice * oi.Quantity),
               QuantitySold = g.Sum(oi => oi.Quantity),
 OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
       })
        .OrderByDescending(x => x.TotalSales)
             .Take(count)
    .ToListAsync();

            return Result<List<TopProductDto>>.Success(topProducts);
        }
        catch (Exception ex)
        {
 return Result<List<TopProductDto>>.InternalError($"Error: {ex.Message}");
  }
    }

    public async Task<Result<List<TopCategoryDto>>> GetTopCategoriesAsync(
        DateTime startDate, DateTime endDate, int count = 10)
    {
 try
        {
  var topCategories = await _unitOfWork.OrderItems
         .SelectAll(oi => !oi.IsDeleted &&
          !oi.Order.IsDeleted &&
                DateTime.Parse(oi.Order.OrderDate) >= startDate &&
  DateTime.Parse(oi.Order.OrderDate) <= endDate,
        new[] { "Order", "Product", "Product.Category" })
        .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
  .Select(g => new TopCategoryDto
    {
            CategoryId = g.Key.CategoryId,
         CategoryName = g.Key.Name,
 TotalSales = g.Sum(oi => oi.UnitPrice * oi.Quantity),
           ProductCount = g.Select(oi => oi.ProductId).Distinct().Count(),
        OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
      })
     .OrderByDescending(x => x.TotalSales)
     .Take(count)
                .ToListAsync();

            return Result<List<TopCategoryDto>>.Success(topCategories);
        }
        catch (Exception ex)
        {
 return Result<List<TopCategoryDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TopCustomerDto>>> GetTopCustomersAsync(
        DateTime startDate, DateTime endDate, int count = 10)
    {
        try
        {
   var topCustomers = await _unitOfWork.Orders
        .SelectAll(o => !o.IsDeleted &&
  DateTime.Parse(o.OrderDate) >= startDate &&
           DateTime.Parse(o.OrderDate) <= endDate,
          new[] { "Customer" })
            .GroupBy(o => new { o.CustomerId, o.Customer.FirstName, o.Customer.LastName })
      .Select(g => new TopCustomerDto
{
   CustomerId = g.Key.CustomerId,
     CustomerName = $"{g.Key.FirstName} {g.Key.LastName}",
  TotalOrders = g.Count(),
        TotalSpent = g.Sum(o => o.FinalPrice)
 })
        .OrderByDescending(x => x.TotalSpent)
   .Take(count)
    .ToListAsync();

       return Result<List<TopCustomerDto>>.Success(topCustomers);
    }
        catch (Exception ex)
        {
  return Result<List<TopCustomerDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TopSellerDto>>> GetTopSellersAsync(
 DateTime startDate, DateTime endDate, int count = 10)
    {
        try
        {
            var topSellers = await _unitOfWork.Orders
  .SelectAll(o => !o.IsDeleted &&
         o.SellerId != null &&
        DateTime.Parse(o.OrderDate) >= startDate &&
     DateTime.Parse(o.OrderDate) <= endDate,
     new[] { "Seller" })
        .GroupBy(o => new { o.SellerId, o.Seller.FirstName, o.Seller.LastName })
         .Select(g => new TopSellerDto
       {
   SellerId = g.Key.SellerId.Value,
             SellerName = $"{g.Key.FirstName} {g.Key.LastName}",
      TotalOrders = g.Count(),
       TotalSales = g.Sum(o => o.FinalPrice)
          })
    .OrderByDescending(x => x.TotalSales)
                .Take(count)
             .ToListAsync();

      return Result<List<TopSellerDto>>.Success(topSellers);
        }
     catch (Exception ex)
        {
            return Result<List<TopSellerDto>>.InternalError($"Error: {ex.Message}");
     }
    }

    public async Task<Result<InventoryReportDto>> GetInventoryReportAsync()
    {
     try
        {
     var products = await _unitOfWork.Products
       .SelectAll(p => !p.IsDeleted, new[] { "Category" })
   .ToListAsync();

var report = new InventoryReportDto
     {
TotalProducts = products.Count,
     TotalValue = products.Sum(p => p.Price * p.StockQuantity),
                LowStockProducts = products.Count(p => p.IsLowStock),
       OutOfStockProducts = products.Count(p => p.IsOutOfStock),
      ProductsByCategory = products
       .GroupBy(p => new { p.CategoryId, p.Category.Name })
      .Select(g => new CategoryInventoryDto
        {
       CategoryId = g.Key.CategoryId,
   CategoryName = g.Key.Name,
 ProductCount = g.Count(),
    TotalValue = g.Sum(p => p.Price * p.StockQuantity),
             LowStockCount = g.Count(p => p.IsLowStock)
                 })
         .OrderByDescending(x => x.TotalValue)
  .ToList()
            };

            return Result<InventoryReportDto>.Success(report);
        }
        catch (Exception ex)
        {
       return Result<InventoryReportDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<LowStockProductDto>>> GetLowStockProductsAsync(int threshold = 5)
    {
     try
        {
       var products = await _unitOfWork.Products
 .SelectAll(p => !p.IsDeleted && p.StockQuantity <= threshold,
         new[] { "Category" })
            .Select(p => new LowStockProductDto
        {
  ProductId = p.Id,
 ProductName = p.Name,
     CategoryName = p.Category.Name,
          CurrentStock = p.StockQuantity,
        MinStockLevel = p.MinStockLevel,
        Price = p.Price,
           IsOutOfStock = p.IsOutOfStock
      })
       .OrderBy(p => p.CurrentStock)
         .ToListAsync();

            return Result<List<LowStockProductDto>>.Success(products);
   }
        catch (Exception ex)
    {
          return Result<List<LowStockProductDto>>.InternalError($"Error: {ex.Message}");
   }
    }

    public async Task<Result<List<ProductMovementDto>>> GetProductMovementsAsync(
        int productId, DateTime startDate, DateTime endDate)
    {
        try
        {
          var orderItems = await _unitOfWork.OrderItems
    .SelectAll(oi => !oi.IsDeleted &&
    !oi.Order.IsDeleted &&
    oi.ProductId == productId &&
        DateTime.Parse(oi.Order.OrderDate) >= startDate &&
     DateTime.Parse(oi.Order.OrderDate) <= endDate,
        new[] { "Order" })
         .Select(oi => new ProductMovementDto
      {
           Date = DateTime.Parse(oi.Order.OrderDate),
          Type = "Sale",
   Quantity = -oi.Quantity,
      UnitPrice = oi.UnitPrice,
      TotalAmount = oi.UnitPrice * oi.Quantity,
    Reference = $"Order #{oi.Order.OrderNumber}"
      })
       .ToListAsync();

            // TODO: Add other movement types (restocks, returns, etc.)

      return Result<List<ProductMovementDto>>.Success(orderItems.OrderByDescending(x => x.Date).ToList());
   }
        catch (Exception ex)
        {
      return Result<List<ProductMovementDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CustomerAnalyticsDto>> GetCustomerAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
   try
        {
            var orders = await _unitOfWork.Orders
        .SelectAll(o => !o.IsDeleted &&
 DateTime.Parse(o.OrderDate) >= startDate &&
  DateTime.Parse(o.OrderDate) <= endDate)
     .ToListAsync();

         var customers = await _unitOfWork.Customers
  .SelectAll(c => !c.IsDeleted)
        .ToListAsync();

            var analytics = new CustomerAnalyticsDto
            {
      TotalCustomers = customers.Count,
        ActiveCustomers = orders.Select(o => o.CustomerId).Distinct().Count(),
      NewCustomers = customers.Count(c => DateTime.Parse(c.CreatedAt) >= startDate),
     AverageOrdersPerCustomer = orders.Any() ? (double)orders.Count / orders.Select(o => o.CustomerId).Distinct().Count() : 0,
     AverageSpendPerCustomer = orders.Any() ? orders.Sum(o => o.FinalPrice) / orders.Select(o => o.CustomerId).Distinct().Count() : 0
            };

          return Result<CustomerAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
            return Result<CustomerAnalyticsDto>.InternalError($"Error: {ex.Message}");
        }
    }

public async Task<Result<SellerAnalyticsDto>> GetSellerAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
        var orders = await _unitOfWork.Orders
    .SelectAll(o => !o.IsDeleted &&
     o.SellerId != null &&
   DateTime.Parse(o.OrderDate) >= startDate &&
     DateTime.Parse(o.OrderDate) <= endDate)
    .ToListAsync();

            var analytics = new SellerAnalyticsDto
          {
         TotalSellers = await _unitOfWork.Sellers.CountAsync(s => !s.IsDeleted),
              ActiveSellers = orders.Select(o => o.SellerId).Distinct().Count(),
     TotalSales = orders.Sum(o => o.FinalPrice),
     AverageOrdersPerSeller = orders.Any() ? (double)orders.Count / orders.Select(o => o.SellerId).Distinct().Count() : 0,
     AverageSalesPerSeller = orders.Any() ? orders.Sum(o => o.FinalPrice) / orders.Select(o => o.SellerId).Distinct().Count() : 0
            };

  return Result<SellerAnalyticsDto>.Success(analytics);
     }
        catch (Exception ex)
     {
            return Result<SellerAnalyticsDto>.InternalError($"Error: {ex.Message}");
     }
 }

    public async Task<Result<CashbackAnalyticsDto>> GetCashbackAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
   try
    {
        var transactions = await _unitOfWork.CashbackTransactions
        .SelectAll(c => !c.IsDeleted &&
        DateTime.Parse(c.CreatedAt) >= startDate &&
        DateTime.Parse(c.CreatedAt) <= endDate)
   .ToListAsync();

            var analytics = new CashbackAnalyticsDto
       {
       TotalEarned = transactions.Where(t => t.Type == Domain.Enums.CashbackTransactionType.Earned)
    .Sum(t => t.Amount),
      TotalUsed = Math.Abs(transactions.Where(t => t.Type == Domain.Enums.CashbackTransactionType.Used)
.Sum(t => t.Amount)),
 TotalExpired = Math.Abs(transactions.Where(t => t.Type == Domain.Enums.CashbackTransactionType.Expired)
    .Sum(t => t.Amount)),
       AverageEarnedPerTransaction = transactions.Where(t => t.Type == Domain.Enums.CashbackTransactionType.Earned)
     .Average(t => t.Amount),
         TransactionCount = transactions.Count
            };

return Result<CashbackAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
     return Result<CashbackAnalyticsDto>.InternalError($"Error: {ex.Message}");
  }
    }

    public async Task<Result<DiscountAnalyticsDto>> GetDiscountAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
    var orders = await _unitOfWork.Orders
       .SelectAll(o => !o.IsDeleted &&
   o.DiscountApplied > 0 &&
   DateTime.Parse(o.OrderDate) >= startDate &&
        DateTime.Parse(o.OrderDate) <= endDate)
   .ToListAsync();

  var analytics = new DiscountAnalyticsDto
      {
   TotalDiscounts = orders.Sum(o => o.DiscountApplied),
        OrdersWithDiscount = orders.Count,
 AverageDiscountAmount = orders.Any() ? orders.Average(o => o.DiscountApplied) : 0,
     TotalDiscountedAmount = orders.Sum(o => o.TotalPrice)
            };

       return Result<DiscountAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
return Result<DiscountAnalyticsDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<MonthlyReportDto>> GetMonthlyReportAsync(int year, int month)
    {
        try
        {
        var startDate = new DateTime(year, month, 1);
         var endDate = startDate.AddMonths(1).AddDays(-1);

            var orders = await _unitOfWork.Orders
      .SelectAll(o => !o.IsDeleted &&
          DateTime.Parse(o.OrderDate) >= startDate &&
       DateTime.Parse(o.OrderDate) <= endDate)
        .ToListAsync();

            var report = new MonthlyReportDto
            {
   Year = year,
                Month = month,
      TotalRevenue = orders.Sum(o => o.FinalPrice),
      TotalOrders = orders.Count,
                TotalDiscounts = orders.Sum(o => o.DiscountApplied),
              AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalPrice) : 0,
            OrdersByDay = orders.GroupBy(o => DateTime.Parse(o.OrderDate).Day)
    .Select(g => new DailyStatsDto
         {
    Day = g.Key,
        Orders = g.Count(),
       Revenue = g.Sum(o => o.FinalPrice)
     })
         .OrderBy(x => x.Day)
   .ToList()
   };

            return Result<MonthlyReportDto>.Success(report);
        }
  catch (Exception ex)
        {
            return Result<MonthlyReportDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<YearlyReportDto>> GetYearlyReportAsync(int year)
    {
     try
        {
            var startDate = new DateTime(year, 1, 1);
       var endDate = startDate.AddYears(1).AddDays(-1);

            var orders = await _unitOfWork.Orders
       .SelectAll(o => !o.IsDeleted &&
   DateTime.Parse(o.OrderDate) >= startDate &&
       DateTime.Parse(o.OrderDate) <= endDate)
    .ToListAsync();

            var report = new YearlyReportDto
            {
      Year = year,
          TotalRevenue = orders.Sum(o => o.FinalPrice),
 TotalOrders = orders.Count,
TotalDiscounts = orders.Sum(o => o.DiscountApplied),
           AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalPrice) : 0,
   OrdersByMonth = orders.GroupBy(o => DateTime.Parse(o.OrderDate).Month)
               .Select(g => new MonthlyStatsDto
  {
        Month = g.Key,
Orders = g.Count(),
             Revenue = g.Sum(o => o.FinalPrice)
         })
         .OrderBy(x => x.Month)
  .ToList()
            };

            return Result<YearlyReportDto>.Success(report);
 }
    catch (Exception ex)
        {
    return Result<YearlyReportDto>.InternalError($"Error: {ex.Message}");
        }
  }

    public async Task<Result<List<CustomReportDto>>> GenerateCustomReportAsync(ReportFilterDto filter)
    {
   try
        {
       var query = _unitOfWork.Orders.Table
            .Include(o => o.Customer)
                .Include(o => o.Seller)
  .Include(o => o.OrderItems)
     .Where(o => !o.IsDeleted);

            // Apply filters
            if (filter.StartDate.HasValue)
     query = query.Where(o => DateTime.Parse(o.OrderDate) >= filter.StartDate.Value);

       if (filter.EndDate.HasValue)
          query = query.Where(o => DateTime.Parse(o.OrderDate) <= filter.EndDate.Value);

            if (filter.MinAmount.HasValue)
   query = query.Where(o => o.FinalPrice >= filter.MinAmount.Value);

if (filter.MaxAmount.HasValue)
                query = query.Where(o => o.FinalPrice <= filter.MaxAmount.Value);

          if (!string.IsNullOrEmpty(filter.Status))
    query = query.Where(o => o.Status.ToString() == filter.Status);

            if (!string.IsNullOrEmpty(filter.PaymentMethod))
   query = query.Where(o => o.PaymentMethod.ToString() == filter.PaymentMethod);

            var orders = await query.ToListAsync();

            // Generate custom report
            var report = orders.Select(o => new CustomReportDto
   {
      OrderId = o.Id,
                OrderNumber = o.OrderNumber,
        OrderDate = DateTime.Parse(o.OrderDate),
                CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
     SellerName = o.Seller != null ? $"{o.Seller.FirstName} {o.Seller.LastName}" : null,
   Status = o.Status.ToString(),
              TotalAmount = o.TotalPrice,
        DiscountAmount = o.DiscountApplied,
                FinalAmount = o.FinalPrice,
    ItemCount = o.OrderItems.Count,
     PaymentMethod = o.PaymentMethod.ToString()
         }).ToList();

    return Result<List<CustomReportDto>>.Success(report);
      }
      catch (Exception ex)
        {
            return Result<List<CustomReportDto>>.InternalError($"Error: {ex.Message}");
        }
    }
}