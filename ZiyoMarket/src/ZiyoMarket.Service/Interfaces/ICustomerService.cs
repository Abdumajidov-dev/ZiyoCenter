using ZiyoMarket.Service.DTOs.Customers;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface ICustomerService
{
    // CRUD
    Task<Result<CustomerDetailDto>> GetCustomerByIdAsync(int customerId);
    Task<Result<PaginationResponse<CustomerListDto>>> GetCustomersAsync(CustomerFilterRequest request);
    Task<Result<CustomerDetailDto>> CreateCustomerAsync(CreateCustomerDto request, int createdBy);
    Task<Result<CustomerDetailDto>> UpdateCustomerAsync(int id, UpdateCustomerDto request, int updatedBy);
    Task<Result> DeleteCustomerAsync(int customerId, int deletedBy);

    // Profile
    Task<Result<CustomerProfileDto>> GetCustomerProfileAsync(int customerId);
    Task<Result<CustomerProfileDto>> UpdateCustomerProfileAsync(int customerId, UpdateCustomerDto request);

    // Search
    Task<Result<List<CustomerListDto>>> SearchCustomersAsync(string searchTerm);
    Task<Result<CustomerDetailDto>> GetCustomerByPhoneAsync(string phone);
    Task<Result<CustomerDetailDto>> GetCustomerByEmailAsync(string email);

    // Statistics
    Task<Result<CustomerStatisticsDto>> GetCustomerStatisticsAsync(int customerId);
    Task<Result<List<TopCustomerDto>>> GetTopCustomersAsync(int count = 10);

    // Status
    Task<Result> ToggleCustomerStatusAsync(int customerId, int updatedBy);

    // Bulk operations
    Task<Result> DeleteAllCustomersAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<CustomerDetailDto>>> SeedMockCustomersAsync(int createdBy, int count = 10);
}
