using ZiyoMarket.Service.DTOs.Payments.Click;

namespace ZiyoMarket.Service.Interfaces;

public interface IClickService
{
    Task<ClickResponseDto> HandleRequestAsync(ClickRequestDto request);
}
