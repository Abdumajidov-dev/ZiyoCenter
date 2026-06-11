using ZiyoMarket.Service.DTOs.Payment;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IPaymentCardService
{
    Task<Result<List<PaymentCardResultDto>>> GetAllAsync();
    Task<Result<PaymentCardResultDto>> GetActiveAsync();
    Task<Result<PaymentCardResultDto>> CreateAsync(CreatePaymentCardDto dto);
    Task<Result<PaymentCardResultDto>> SetActiveAsync(int id);
    Task<Result> DeleteAsync(int id);
}
