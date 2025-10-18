﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Service.DTOs.Customers;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDetailDto>> GetCustomerByIdAsync(int customerId)
    {
        try
        {
            var customer = await _unitOfWork.Customers
                .SelectAsync(c => c.Id == customerId && !c.IsDeleted);

            if (customer == null)
                return Result<CustomerDetailDto>.NotFound("Customer not found");

            var dto = _mapper.Map<CustomerDetailDto>(customer);

            // Add statistics
            dto.TotalOrders = await _unitOfWork.Orders.CountAsync(o => o.CustomerId == customerId && !o.IsDeleted);
            dto.TotalSpent = await _unitOfWork.Orders
                .SelectAll(o => o.CustomerId == customerId && !o.IsDeleted)
                .SumAsync(o => o.FinalPrice);

            return Result<CustomerDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<CustomerDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PaginationResponse<CustomerListDto>>> GetCustomersAsync(CustomerFilterRequest request)
    {
        try
        {
            var query = _unitOfWork.Customers.Table.Where(c => !c.IsDeleted);

            // Search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(term) ||
                    c.LastName.ToLower().Contains(term) ||
                    c.Phone.Contains(term) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)));
            }

            // Status filter
            if (request.IsActive.HasValue)
                query = query.Where(c => c.IsActive == request.IsActive.Value);

            query = query.OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync();
            var customers = await query
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<CustomerListDto>>(customers);

            // Add order counts
            foreach (var dto in dtos)
            {
                dto.TotalOrders = await _unitOfWork.Orders
                    .CountAsync(o => o.CustomerId == dto.Id && !o.IsDeleted);
            }

            return Result<PaginationResponse<CustomerListDto>>.Success(
                new PaginationResponse<CustomerListDto>(dtos, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex)
        {
            return Result<PaginationResponse<CustomerListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CustomerDetailDto>> CreateCustomerAsync(CreateCustomerDto request, int createdBy)
    {
        try
        {
            // Check if phone exists
            var existingPhone = await _unitOfWork.Customers
                .AnyAsync(c => c.Phone == request.Phone && !c.IsDeleted);

            if (existingPhone)
                return Result<CustomerDetailDto>.Conflict("Phone number already exists");

            // Check if email exists
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingEmail = await _unitOfWork.Customers
                    .AnyAsync(c => c.Email == request.Email && !c.IsDeleted);

                if (existingEmail)
                    return Result<CustomerDetailDto>.Conflict("Email already exists");
            }

            var customer = _mapper.Map<Customer>(request);
            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password ?? "123456");
            customer.CashbackBalance = 0;
            customer.IsActive = true;
            customer.CreatedBy = createdBy;

            await _unitOfWork.Customers.InsertAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CustomerDetailDto>(customer);
            return Result<CustomerDetailDto>.Success(dto, "Customer created successfully", 201);
        }
        catch (Exception ex)
        {
            return Result<CustomerDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CustomerDetailDto>> UpdateCustomerAsync(int id, UpdateCustomerDto request, int updatedBy)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return Result<CustomerDetailDto>.NotFound("Customer not found");

            // Check phone uniqueness
            var phoneExists = await _unitOfWork.Customers
                .AnyAsync(c => c.Phone == request.Phone && c.Id != id && !c.IsDeleted);

            if (phoneExists)
                return Result<CustomerDetailDto>.Conflict("Phone number already exists");

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Phone = request.Phone;
            customer.Email = request.Email;
            customer.Address = request.Address;
            customer.UpdatedBy = updatedBy;
            customer.MarkAsUpdated();

            await _unitOfWork.Customers.Update(customer, id);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CustomerDetailDto>(customer);
            return Result<CustomerDetailDto>.Success(dto, "Customer updated successfully");
        }
        catch (Exception ex)
        {
            return Result<CustomerDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteCustomerAsync(int customerId, int deletedBy)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result.NotFound("Customer not found");

            customer.DeletedBy = deletedBy;
            customer.Delete();

            await _unitOfWork.Customers.Update(customer, customerId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Customer deleted successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CustomerProfileDto>> GetCustomerProfileAsync(int customerId)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result<CustomerProfileDto>.NotFound("Customer not found");

            var dto = _mapper.Map<CustomerProfileDto>(customer);

            // Add statistics
            dto.TotalOrders = await _unitOfWork.Orders.CountAsync(o => o.CustomerId == customerId && !o.IsDeleted);
            dto.TotalSpent = await _unitOfWork.Orders
                .SelectAll(o => o.CustomerId == customerId && !o.IsDeleted)
                .SumAsync(o => o.FinalPrice);

            return Result<CustomerProfileDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<CustomerProfileDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CustomerProfileDto>> UpdateCustomerProfileAsync(int customerId, UpdateCustomerDto request)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result<CustomerProfileDto>.NotFound("Customer not found");

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Email = request.Email;
            customer.Address = request.Address;
            customer.UpdatedBy = customerId;
            customer.MarkAsUpdated();

            await _unitOfWork.Customers.Update(customer, customerId);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<CustomerProfileDto>(customer);
            return Result<CustomerProfileDto>.Success(dto, "Profile updated successfully");
        }
        catch (Exception ex)
        {
            return Result<CustomerProfileDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CustomerListDto>>> SearchCustomersAsync(string searchTerm)
    {
        try
        {
            var term = searchTerm.ToLower();
            var customers = await _unitOfWork.Customers.Table
                .Where(c => !c.IsDeleted &&
                    (c.FirstName.ToLower().Contains(term) ||
                     c.LastName.ToLower().Contains(term) ||
                     c.Phone.Contains(term) ||
                     (c.Email != null && c.Email.ToLower().Contains(term))))
                .Take(20)
                .ToListAsync();

            var dtos = _mapper.Map<List<CustomerListDto>>(customers);
            return Result<List<CustomerListDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CustomerListDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CustomerDetailDto>> GetCustomerByPhoneAsync(string phone)
    {
        try
        {
            var customer = await _unitOfWork.Customers
                .SelectAsync(c => c.Phone == phone && !c.IsDeleted);

            if (customer == null)
                return Result<CustomerDetailDto>.NotFound("Customer not found");

            var dto = _mapper.Map<CustomerDetailDto>(customer);
            return Result<CustomerDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<CustomerDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CustomerDetailDto>> GetCustomerByEmailAsync(string email)
    {
        try
        {
            var customer = await _unitOfWork.Customers
                .SelectAsync(c => c.Email == email && !c.IsDeleted);

            if (customer == null)
                return Result<CustomerDetailDto>.NotFound("Customer not found");

            var dto = _mapper.Map<CustomerDetailDto>(customer);
            return Result<CustomerDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<CustomerDetailDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CustomerPersonalStatsDto>> GetCustomerStatisticsAsync(int customerId)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result<CustomerPersonalStatsDto>.NotFound("Customer not found");

            var orders = await _unitOfWork.Orders
                .SelectAll(o => o.CustomerId == customerId && !o.IsDeleted)
                .ToListAsync();

            var stats = new CustomerPersonalStatsDto
            {
                CustomerId = customerId,
                TotalOrders = orders.Count,
                TotalSpent = orders.Sum(o => o.FinalPrice),
                TotalCashback = customer.CashbackBalance,
                AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalPrice) : 0,
                LastOrderDate = orders.Any() ? DateTime.Parse(orders.OrderByDescending(o => o.OrderDate).First().OrderDate) : (DateTime?)null
            };

            return Result<CustomerPersonalStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<CustomerPersonalStatsDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TopCustomerDto>>> GetTopCustomersAsync(int count = 10)
    {
        try
        {
            var customerOrders = await _unitOfWork.Orders
                .SelectAll(o => !o.IsDeleted, new[] { "Customer" })
                .GroupBy(o => o.CustomerId)
                .Select(g => new TopCustomerDto
                {
                    CustomerId = g.Key,
                    CustomerName = $"{g.First().Customer.FirstName} {g.First().Customer.LastName}",
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.FinalPrice)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(count)
                .ToListAsync();

            return Result<List<TopCustomerDto>>.Success(customerOrders);
        }
        catch (Exception ex)
        {
            return Result<List<TopCustomerDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ToggleCustomerStatusAsync(int customerId, int updatedBy)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result.NotFound("Customer not found");

            customer.IsActive = !customer.IsActive;
            customer.UpdatedBy = updatedBy;
            customer.MarkAsUpdated();

            await _unitOfWork.Customers.Update(customer, customerId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"Customer {(customer.IsActive ? "activated" : "deactivated")}");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAllCustomersAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _unitOfWork.Customers.Table.Where(c => !c.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(c => DateTime.Parse(c.CreatedAt) >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => DateTime.Parse(c.CreatedAt) <= endDate.Value);

            var customers = await query.ToListAsync();

            foreach (var customer in customers)
            {
                customer.DeletedBy = deletedBy;
                customer.Delete();
                await _unitOfWork.Customers.Update(customer, customer.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success($"{customers.Count} customers deleted");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CustomerDetailDto>>> SeedMockCustomersAsync(int createdBy, int count = 10)
    {
        try
        {
            var random = new Random();
            var customers = new List<Customer>();

            var firstNames = new[] { "Aziz", "Olim", "Dilshod", "Sardor", "Jamshid", "Shohruh", "Bekzod", "Otabek", "Jasur", "Kamol" };
            var lastNames = new[] { "Karimov", "Rahimov", "Tursunov", "Alimov", "Nazarov", "Yusupov", "Sharipov", "Mahmudov", "Ergashev", "Ismoilov" };

            for (int i = 0; i < count; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];

                var customer = new Customer
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = $"+998{random.Next(90, 99)}{random.Next(1000000, 9999999)}",
                    Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Address = $"Tashkent, Street {random.Next(1, 100)}",
                    CashbackBalance = random.Next(0, 50000),
                    IsActive = true,
                    CreatedBy = createdBy
                };

                await _unitOfWork.Customers.InsertAsync(customer);
                customers.Add(customer);
            }

            await _unitOfWork.SaveChangesAsync();

            var dtos = _mapper.Map<List<CustomerDetailDto>>(customers);
            return Result<List<CustomerDetailDto>>.Success(dtos, $"{count} mock customers created");
        }
        catch (Exception ex)
        {
            return Result<List<CustomerDetailDto>>.InternalError($"Error: {ex.Message}");
        }
    }


    Task<Result<CustomerStatisticsDto>> ICustomerService.GetCustomerStatisticsAsync(int customerId)
    {
        throw new NotImplementedException();
    }
}