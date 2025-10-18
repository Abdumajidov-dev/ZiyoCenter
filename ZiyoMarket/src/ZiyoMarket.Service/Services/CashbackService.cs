using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Cashback;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class CashbackService : ICashbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CashbackService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CashbackSummaryDto>> GetCashbackSummaryAsync(int customerId)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result<CashbackSummaryDto>.NotFound("Customer not found");

            var transactions = await _unitOfWork.CashbackTransactions
                .SelectAll(c => c.CustomerId == customerId && !c.IsDeleted)
                .ToListAsync();

            // Muddati tugayotgan cashback ni olish
            var expiringResult = await GetExpiringCashbackAsync(customerId, 7);

            var summary = new CashbackSummaryDto
            {
                TotalBalance = customer.CashbackBalance,
                TotalEarned = transactions
                    .Where(t => t.Type == CashbackTransactionType.Earned)
                    .Sum(t => t.Amount),
                TotalUsed = transactions
                    .Where(t => t.Type == CashbackTransactionType.Used)
                    .Sum(t => Math.Abs(t.Amount)),
                TotalExpired = transactions
                    .Where(t => t.Type == CashbackTransactionType.Expired)
                    .Sum(t => Math.Abs(t.Amount)),
                ExpiringIn7Days = expiringResult.Data // ✅ To'g'rilandi
            };

            return Result<CashbackSummaryDto>.Success(summary); // ✅ To'g'rilandi
        }
        catch (Exception ex)
        {
            return Result<CashbackSummaryDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CashbackTransactionDto>>> GetCashbackHistoryAsync(
        int customerId, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var transactions = await _unitOfWork.CashbackTransactions
                .SelectAll(c => c.CustomerId == customerId && !c.IsDeleted, new[] { "Order" })
                .OrderByDescending(c => c.EarnedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<CashbackTransactionDto>>(transactions);
            return Result<List<CashbackTransactionDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CashbackTransactionDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> EarnCashbackAsync(int customerId, int orderId, decimal amount)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result.NotFound("Customer not found");

            // 2% cashback
            var cashbackAmount = Math.Round(amount * 0.02m, 2);

            var transaction = new CashbackTransaction
            {
                CustomerId = customerId,
                OrderId = orderId,
                Type = CashbackTransactionType.Earned,
                Amount = cashbackAmount,
                RemainingAmount = cashbackAmount,
                EarnedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                ExpiresAt = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss"),
                Description = $"Earned from Order #{orderId}"
            };

            await _unitOfWork.CashbackTransactions.InsertAsync(transaction);

            customer.AddCashback(cashbackAmount);
            await _unitOfWork.Customers.Update(customer, customerId);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"Earned {cashbackAmount:N2} so'm cashback");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> UseCashbackAsync(int customerId, int orderId, decimal amount)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result.NotFound("Customer not found");

            if (customer.CashbackBalance < amount)
                return Result.BadRequest($"Insufficient cashback balance. Available: {customer.CashbackBalance:N2}, Requested: {amount:N2}");

            // FIFO: Eng eski cashback'ni birinchi ishlatish
            var availableCashback = await _unitOfWork.CashbackTransactions
                .SelectAll(c => c.CustomerId == customerId &&
                               c.RemainingAmount > 0 &&
                               DateTime.Parse(c.ExpiresAt) > DateTime.UtcNow &&
                               !c.IsDeleted)
                .OrderBy(c => c.ExpiresAt) // FIFO
                .ToListAsync();

            decimal remainingToUse = amount;

            foreach (var transaction in availableCashback)
            {
                if (remainingToUse <= 0) break;

                var amountToUse = Math.Min(transaction.RemainingAmount, remainingToUse);

                // Ishlatilgan cashback transaction yaratish
                var usageTransaction = new CashbackTransaction
                {
                    CustomerId = customerId,
                    OrderId = orderId,
                    Type = CashbackTransactionType.Used,
                    Amount = -amountToUse, // Manfiy qiymat
                    RemainingAmount = 0,
                    EarnedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    ExpiresAt = DateTime.UtcNow.AddYears(1).ToString("yyyy-MM-dd HH:mm:ss"),
                    UsedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = $"Used for Order #{orderId}"
                };

                await _unitOfWork.CashbackTransactions.InsertAsync(usageTransaction);

                // Original transaction'ni yangilash
                transaction.RemainingAmount -= amountToUse;
                if (transaction.RemainingAmount == 0)
                    transaction.UsedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                await _unitOfWork.CashbackTransactions.Update(transaction, transaction.Id);

                remainingToUse -= amountToUse;
            }

            // Mijoz balansini kamaytirish
            customer.UseCashback(amount);
            await _unitOfWork.Customers.Update(customer, customerId);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"Successfully used {amount:N2} so'm cashback");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ExpireCashbackAsync()
    {
        try
        {
            var expiredTransactions = await _unitOfWork.CashbackTransactions
                .SelectAll(c => c.RemainingAmount > 0 &&
                               DateTime.Parse(c.ExpiresAt) <= DateTime.UtcNow &&
                               !c.IsDeleted)
                .ToListAsync();

            foreach (var transaction in expiredTransactions)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(transaction.CustomerId);
                if (customer == null) continue;

                // Muddati o'tgan cashback transaction yaratish
                var expiryTransaction = new CashbackTransaction
                {
                    CustomerId = transaction.CustomerId,
                    Type = CashbackTransactionType.Expired,
                    Amount = -transaction.RemainingAmount,
                    RemainingAmount = 0,
                    EarnedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    ExpiresAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = "Expired cashback"
                };

                await _unitOfWork.CashbackTransactions.InsertAsync(expiryTransaction);

                // Mijoz balansidan ayirish
                customer.UseCashback(transaction.RemainingAmount);

                // Original transaction'ni yangilash
                transaction.RemainingAmount = 0;
                await _unitOfWork.CashbackTransactions.Update(transaction, transaction.Id);
                await _unitOfWork.Customers.Update(customer, customer.Id);
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"{expiredTransactions.Count} cashback transactions expired");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> GetAvailableCashbackAsync(int customerId)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result<decimal>.NotFound("Customer not found");

            return Result<decimal>.Success(customer.CashbackBalance);
        }
        catch (Exception ex)
        {
            return Result<decimal>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> GetExpiringCashbackAsync(int customerId, int daysThreshold = 7)
    {
        try
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysThreshold);

            var expiringAmount = await _unitOfWork.CashbackTransactions
                .SelectAll(c => c.CustomerId == customerId &&
                               c.RemainingAmount > 0 &&
                               DateTime.Parse(c.ExpiresAt) <= expiryDate &&
                               DateTime.Parse(c.ExpiresAt) > DateTime.UtcNow && // ✅ Hali muddati o'tmagan
                               !c.IsDeleted)
                .SumAsync(c => c.RemainingAmount);

            return Result<decimal>.Success(expiringAmount);
        }
        catch (Exception ex)
        {
            return Result<decimal>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAllCashbackTransactionsAsync(
        int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _unitOfWork.CashbackTransactions.Table.Where(c => !c.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(c => DateTime.Parse(c.EarnedAt) >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => DateTime.Parse(c.EarnedAt) <= endDate.Value);

            var transactions = await query.ToListAsync();

            foreach (var transaction in transactions)
            {
                transaction.Delete();
                await _unitOfWork.CashbackTransactions.Update(transaction, transaction.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success($"{transactions.Count} cashback transactions deleted");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CashbackTransactionDto>>> SeedMockCashbackAsync(
        int customerId, int count = 10)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result<List<CashbackTransactionDto>>.NotFound("Customer not found");

            var random = new Random();
            var transactions = new List<CashbackTransaction>();

            for (int i = 0; i < count; i++)
            {
                var amount = random.Next(1000, 10000);
                var earnedDate = DateTime.UtcNow.AddDays(-random.Next(0, 60));

                var transaction = new CashbackTransaction
                {
                    CustomerId = customerId,
                    Type = CashbackTransactionType.Earned,
                    Amount = amount,
                    RemainingAmount = amount,
                    EarnedAt = earnedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ExpiresAt = earnedDate.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss"),
                    Description = $"Mock cashback #{i + 1}"
                };

                await _unitOfWork.CashbackTransactions.InsertAsync(transaction);
                transactions.Add(transaction);

                customer.AddCashback(amount);
            }

            await _unitOfWork.Customers.Update(customer, customerId);
            await _unitOfWork.SaveChangesAsync();

            var dtos = _mapper.Map<List<CashbackTransactionDto>>(transactions);
            return Result<List<CashbackTransactionDto>>.Success(
                dtos,
                $"{count} mock cashback transactions created");
        }
        catch (Exception ex)
        {
            return Result<List<CashbackTransactionDto>>.InternalError($"Error: {ex.Message}");
        }
    }
}