using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Systems;
using ZiyoMarket.Service.DTOs.Payment;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class PaymentCardService : IPaymentCardService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentCardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<PaymentCardResultDto>>> GetAllAsync()
    {
        try
        {
            var cards = await _unitOfWork.PaymentCards.GetAllAsync();
            var dtos = cards.Select(ToDto).ToList();
            return Result<List<PaymentCardResultDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<PaymentCardResultDto>>.InternalError(ex.Message);
        }
    }

    public async Task<Result<PaymentCardResultDto>> GetActiveAsync()
    {
        try
        {
            var card = await _unitOfWork.PaymentCards.FirstOrDefaultAsync(c => c.IsActive);
            if (card is null)
                return Result<PaymentCardResultDto>.NotFound("Aktiv karta topilmadi");

            return Result<PaymentCardResultDto>.Success(ToDto(card));
        }
        catch (Exception ex)
        {
            return Result<PaymentCardResultDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result<PaymentCardResultDto>> CreateAsync(CreatePaymentCardDto dto)
    {
        try
        {
            var card = new PaymentCard
            {
                CardNumber = dto.CardNumber.Replace(" ", ""),
                CardHolder = dto.CardHolder.Trim(),
                BankName = dto.BankName.Trim(),
                Note = dto.Note?.Trim(),
                IsActive = false
            };

            await _unitOfWork.PaymentCards.InsertAsync(card);
            await _unitOfWork.SaveChangesAsync();

            return Result<PaymentCardResultDto>.Success(ToDto(card), "Karta yaratildi", 201);
        }
        catch (Exception ex)
        {
            return Result<PaymentCardResultDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result<PaymentCardResultDto>> SetActiveAsync(int id)
    {
        try
        {
            var card = await _unitOfWork.PaymentCards.GetByIdAsync(id);
            if (card is null)
                return Result<PaymentCardResultDto>.NotFound($"Karta topilmadi (id={id})");

            // Deactivate all cards first
            var allCards = await _unitOfWork.PaymentCards.GetAllAsync();
            foreach (var c in allCards.Where(c => c.IsActive))
            {
                c.IsActive = false;
                _unitOfWork.PaymentCards.Update(c, c.Id);
            }

            card.IsActive = true;
            _unitOfWork.PaymentCards.Update(card, card.Id);
            await _unitOfWork.SaveChangesAsync();

            return Result<PaymentCardResultDto>.Success(ToDto(card));
        }
        catch (Exception ex)
        {
            return Result<PaymentCardResultDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var card = await _unitOfWork.PaymentCards.GetByIdAsync(id);
            if (card is null)
                return Result.NotFound($"Karta topilmadi (id={id})");

            _unitOfWork.PaymentCards.SoftDelete(card);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Karta o'chirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError(ex.Message);
        }
    }

    private static PaymentCardResultDto ToDto(PaymentCard card) => new()
    {
        Id = card.Id,
        CardNumber = card.CardNumber,
        MaskedCardNumber = card.MaskedCardNumber,
        CardHolder = card.CardHolder,
        BankName = card.BankName,
        Note = card.Note,
        IsActive = card.IsActive,
        CreatedAt = card.CreatedAt
    };
}
