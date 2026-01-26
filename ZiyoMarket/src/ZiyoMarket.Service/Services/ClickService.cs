using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiyoMarket.Data.Context;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Payments.Click;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Service.Services;

public class ClickService : IClickService
{
    private readonly ZiyoMarketDbContext _dbContext;
    private readonly ILogger<ClickService> _logger;
    private readonly ClickSettings _settings;

    public ClickService(
        ZiyoMarketDbContext dbContext,
        ILogger<ClickService> logger,
        IOptions<ClickSettings> settings)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<ClickResponseDto> HandleRequestAsync(ClickRequestDto request)
    {
        _logger.LogInformation("Click request received: Action={Action}, TransId={TransId}, Amount={Amount}",
            request.Action, request.ClickTransId, request.Amount);

        // 1. Check Signature
        if (!CheckSignature(request))
        {
            _logger.LogWarning("Click signature check failed for TransId={TransId}", request.ClickTransId);
            return CreateErrorResponse(request, -1, "SIGN CHECK FAILED!");
        }

        // 2. Handle Action
        request.Action = CleanAction(request.Action);

        if (request.Action == 0) // Prepare
        {
            return await HandlePrepareAsync(request);
        }
        else if (request.Action == 1) // Complete
        {
            return await HandleCompleteAsync(request);
        }

        return CreateErrorResponse(request, -3, "Action not found");
    }

    private async Task<ClickResponseDto> HandlePrepareAsync(ClickRequestDto request)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == request.MerchantTransId);
        
        // 1. Transaction exists check (Order check)
        if (order == null)
        {
            return CreateErrorResponse(request, -5, "User/Order does not exist");
        }

        // 2. Amount check
        // Allow small difference for float precision if needed, but usually exact match required
        if (Math.Abs((decimal)request.Amount - order.FinalPrice) > 0.01m)
        {
            return CreateErrorResponse(request, -2, "Incorrect parameter amount");
        }

        // 3. Already paid check
        if (order.IsPaid)
        {
            return CreateErrorResponse(request, -4, "Already paid");
        }

        // 4. Cancelled check
        if (order.IsCancelled)
        {
            return CreateErrorResponse(request, -9, "Transaction cancelled");
        }

        // Success Prepare
        return new ClickResponseDto
        {
            ClickTransId = request.ClickTransId,
            MerchantTransId = request.MerchantTransId,
            MerchantPrepareId = order.Id, // We can use OrderId as prepare ID or generate a temp one
            Error = 0,
            ErrorNote = "Success"
        };
    }

    private async Task<ClickResponseDto> HandleCompleteAsync(ClickRequestDto request)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == request.MerchantTransId);

        // 1. Transaction exists check
        if (order == null)
        {
            return CreateErrorResponse(request, -5, "User/Order does not exist");
        }

        // 2. Transaction does not exist (Prepare ID check)
        // In this simple implementation, MerchantPrepareId is Order ID. 
        // If you store prepare transactions in a separate table, check that.
        if (request.MerchantPrepareId != order.Id) 
        {
            return CreateErrorResponse(request, -6, "Transaction does not exist");
        }

        // 3. Already paid check
        if (order.IsPaid)
        {
            // If it's the same transaction, return success (Idempotency)
             if (order.PaymentReference == request.ClickTransId.ToString())
             {
                 return new ClickResponseDto
                 {
                     ClickTransId = request.ClickTransId,
                     MerchantTransId = request.MerchantTransId,
                     MerchantConfirmId = order.Id,
                     Error = 0,
                     ErrorNote = "Success"
                 };
             }
            return CreateErrorResponse(request, -4, "Already paid");
        }

        // 4. Amount check (Safety)
        if (Math.Abs((decimal)request.Amount - order.FinalPrice) > 0.01m)
        {
            return CreateErrorResponse(request, -2, "Incorrect parameter amount");
        }

        // 5. Cancelled check
        if (order.IsCancelled)
        {
            return CreateErrorResponse(request, -9, "Transaction cancelled");
        }

        try
        {
            // Update Order
            order.ProcessPayment(PaymentMethod.Click, request.ClickTransId.ToString());
            
            // If offline/pickup, maybe status change logic is different, but logic says:
            // ProcessPayment usually sets PaidAt. 
            // We might also want to auto-confirm if it was Pending?
            if (order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Confirmed;
            }

            await _dbContext.SaveChangesAsync();

            return new ClickResponseDto
            {
                ClickTransId = request.ClickTransId,
                MerchantTransId = request.MerchantTransId,
                MerchantConfirmId = order.Id,
                Error = 0,
                ErrorNote = "Success"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing Click transaction");
            return CreateErrorResponse(request, -7, "Failed to update user");
        }
    }

    private bool CheckSignature(ClickRequestDto request)
    {
        // Algorithm: md5(click_trans_id + service_id + SECRET_KEY + merchant_trans_id + [merchant_prepare_id] + amount + action + sign_time)
        
        var secKey = _settings.SecretKey;
        
        string source;
        if (request.Action == 0) // Prepare
        {
            source = $"{request.ClickTransId}{request.ServiceId}{secKey}{request.MerchantTransId}{request.Amount}{request.Action}{request.SignTime}";
        }
        else // Complete (includes merchant_prepare_id)
        {
            source = $"{request.ClickTransId}{request.ServiceId}{secKey}{request.MerchantTransId}{request.MerchantPrepareId}{request.Amount}{request.Action}{request.SignTime}";
        }

        var mySign = GetMd5Hash(source);

        // Compare ignoring case
        return string.Equals(mySign, request.SignString, StringComparison.OrdinalIgnoreCase);
    }

    private string GetMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    private ClickResponseDto CreateErrorResponse(ClickRequestDto request, int errorCode, string errorNote)
    {
        return new ClickResponseDto
        {
            ClickTransId = request.ClickTransId,
            MerchantTransId = request.MerchantTransId,
            MerchantPrepareId = request.MerchantPrepareId,
            Error = errorCode,
            ErrorNote = errorNote
        };
    }
    
    private int CleanAction(int action)
    {
        // Sometimes JSON comes with action as string ??? No, purely int in DTO
        return action;
    }
}
