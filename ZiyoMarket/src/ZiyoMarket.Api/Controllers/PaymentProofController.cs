using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Api.Helpers;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Payment;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Manual Payment Verification Controller
/// Bank o'tkazmasi orqali to'lovni tasdiqlash
/// </summary>
[Route("api/payment-proof")]
[ApiController]
public class PaymentProofController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PaymentProofController> _logger;

    public PaymentProofController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IWebHostEnvironment env,
        ILogger<PaymentProofController> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Admin karta ma'lumotlarini olish (Public - hamma ko'ra oladi)
    /// </summary>
    [HttpGet("bank-transfer-info")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBankTransferInfo()
    {
        try
        {
            // SystemSettings'dan admin karta ma'lumotlarini olish
            var cardNumberSetting = await _unitOfWork.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "Payment.BankTransfer.CardNumber" && s.DeletedAt == null);

            var cardHolderSetting = await _unitOfWork.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "Payment.BankTransfer.CardHolderName" && s.DeletedAt == null);

            var bankNameSetting = await _unitOfWork.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "Payment.BankTransfer.BankName" && s.DeletedAt == null);

            var info = new BankTransferInfoDto
            {
                CardNumber = cardNumberSetting?.SettingValue ?? "To'ldirilmagan",
                CardHolderName = cardHolderSetting?.SettingValue ?? "To'ldirilmagan",
                BankName = bankNameSetting?.SettingValue ?? "To'ldirilmagan",
                Instructions = "Ushbu karta raqamiga pul o'tkazing va to'lov isbotini (screenshot) yuklang"
            };

            return SuccessResponse(info, "Admin karta ma'lumotlari");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetBankTransferInfo xatolik");
            return ErrorResponse($"Xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// To'lov isbotini yuklash (Customer)
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadPaymentProof([FromForm] SubmitPaymentProofDto dto)
    {
        try
        {
            if (!IsAuthenticated())
                return ErrorResponse("Tizimga kiring");

            var customerId = GetCurrentUserId();

            // Buyurtmani tekshirish
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
                return ErrorResponse("Buyurtma topilmadi");

            if (order.CustomerId != customerId)
                return ErrorResponse("Bu buyurtma sizga tegishli emas");

            if (order.PaymentMethod != PaymentMethod.BankTransfer)
                return ErrorResponse("Bu buyurtma bank o'tkazmasi orqali to'lanmaydi");

            if (order.PaymentStatus == PaymentStatus.Verified)
                return ErrorResponse("Bu buyurtma allaqachon tasdiqlangan");

            // Screenshot yuklash (agar bor bo'lsa)
            string? imageUrl = null;
            if (dto.ProofImage != null)
            {
                imageUrl = await SavePaymentProofImage(dto.ProofImage, dto.OrderId);
            }

            // Agar screenshot ham, transaction reference ham yo'q bo'lsa
            if (string.IsNullOrWhiteSpace(imageUrl) && string.IsNullOrWhiteSpace(dto.TransactionReference))
            {
                return ErrorResponse("To'lov isboti (screenshot yoki transaction reference) kiritilishi shart");
            }

            // PaymentProof yaratish
            var paymentProof = _mapper.Map<PaymentProof>(dto);
            paymentProof.CustomerId = customerId;
            paymentProof.ProofImageUrl = imageUrl;
            paymentProof.Status = PaymentStatus.UnderReview;
            paymentProof.CreatedBy = customerId;

            await _unitOfWork.PaymentProofs.InsertAsync(paymentProof);

            // Order'ning PaymentStatus'ini yangilash
            order.PaymentStatus = PaymentStatus.UnderReview;
            order.UpdatedBy = customerId;
            order.MarkAsUpdated();
            await _unitOfWork.Orders.UpdateAsync(order);

            await _unitOfWork.SaveChangesAsync();

            var result = new SubmitPaymentProofResultDto
            {
                PaymentProofId = paymentProof.Id,
                OrderId = order.Id,
                Status = PaymentStatus.UnderReview,
                StatusText = "Ko'rib chiqilmoqda",
                UploadedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                EstimatedReviewTime = "1-24 soat"
            };

            return SuccessResponse(result, "To'lov isboti yuklandi. Admin tasdiqlashi kutilmoqda.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadPaymentProof xatolik");
            return ErrorResponse($"Xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// O'z to'lov isbotlarini ko'rish (Customer)
    /// </summary>
    [HttpGet("my-proofs")]
    [Authorize]
    public async Task<IActionResult> GetMyPaymentProofs([FromQuery] int? orderId = null)
    {
        try
        {
            if (!IsAuthenticated())
                return ErrorResponse("Tizimga kiring");

            var customerId = GetCurrentUserId();

            var proofs = await _unitOfWork.PaymentProofs.SelectAllAsync(
                p => p.CustomerId == customerId && p.DeletedAt == null &&
                     (orderId == null || p.OrderId == orderId),
                includes: new[] { "Order" }
            );

            var result = _mapper.Map<List<PaymentProofResultDto>>(proofs.OrderByDescending(p => p.CreatedAt));

            return SuccessResponse(result, $"{result.Count} ta to'lov isboti topildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyPaymentProofs xatolik");
            return ErrorResponse($"Xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Tasdiqlash kutayotgan to'lovlar (Admin)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetPendingPaymentProofs()
    {
        try
        {
            var proofs = await _unitOfWork.PaymentProofs.SelectAllAsync(
                p => p.Status == PaymentStatus.UnderReview && p.DeletedAt == null,
                includes: new[] { "Order.Customer" }
            );

            var result = _mapper.Map<List<PaymentProofResultDto>>(proofs.OrderBy(p => p.CreatedAt));

            var stats = new
            {
                total_count = result.Count,
                pending_amount = result.Sum(p => p.Amount)
            };

            return SuccessResponse(new { proofs = result, stats }, $"{result.Count} ta kutayotgan to'lov");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPendingPaymentProofs xatolik");
            return ErrorResponse($"Xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// To'lovni tasdiqlash (Admin)
    /// </summary>
    [HttpPost("verify/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> VerifyPaymentProof(int id, [FromBody] VerifyPaymentProofDto dto)
    {
        try
        {
            if (!IsAuthenticated())
                return ErrorResponse("Tizimga kiring");

            var adminId = GetCurrentUserId();

            var paymentProof = await _unitOfWork.PaymentProofs.GetByIdAsync(id, includes: new[] { "Order" });
            if (paymentProof == null)
                return ErrorResponse("To'lov isboti topilmadi");

            if (paymentProof.Status != PaymentStatus.UnderReview)
                return ErrorResponse("Faqat ko'rib chiqilayotgan to'lovni tasdiqlash mumkin");

            // PaymentProof'ni tasdiqlash
            paymentProof.Approve(adminId, dto.AdminNotes);
            await _unitOfWork.PaymentProofs.UpdateAsync(paymentProof);

            // Order'ni yangilash
            var order = paymentProof.Order;
            if (order != null)
            {
                order.VerifyPayment(adminId);
                await _unitOfWork.Orders.UpdateAsync(order);
            }

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<PaymentProofResultDto>(paymentProof);

            return SuccessResponse(result, "To'lov tasdiqlandi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyPaymentProof xatolik: {Message}", ex.Message);
            return ErrorResponse($"Xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// To'lovni rad etish (Admin)
    /// </summary>
    [HttpPost("reject/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RejectPaymentProof(int id, [FromBody] RejectPaymentProofDto dto)
    {
        try
        {
            if (!IsAuthenticated())
                return ErrorResponse("Tizimga kiring");

            var adminId = GetCurrentUserId();

            var paymentProof = await _unitOfWork.PaymentProofs.GetByIdAsync(id, includes: new[] { "Order" });
            if (paymentProof == null)
                return ErrorResponse("To'lov isboti topilmadi");

            if (paymentProof.Status != PaymentStatus.UnderReview)
                return ErrorResponse("Faqat ko'rib chiqilayotgan to'lovni rad etish mumkin");

            if (string.IsNullOrWhiteSpace(dto.AdminNotes))
                return ErrorResponse("Rad etish sababi kiritilishi shart");

            // PaymentProof'ni rad etish
            paymentProof.Reject(adminId, dto.AdminNotes);
            await _unitOfWork.PaymentProofs.UpdateAsync(paymentProof);

            // Order'ni yangilash
            var order = paymentProof.Order;
            if (order != null)
            {
                order.RejectPayment(adminId, dto.AdminNotes);
                await _unitOfWork.Orders.UpdateAsync(order);
            }

            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<PaymentProofResultDto>(paymentProof);

            return SuccessResponse(result, "To'lov rad etildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RejectPaymentProof xatolik: {Message}", ex.Message);
            return ErrorResponse($"Xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Barcha to'lov isbotlari (Admin) - Pagination bilan
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAllPaymentProofs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PaymentStatus? status = null)
    {
        try
        {
            // Get total count
            var totalCount = await _unitOfWork.PaymentProofs.CountAsync(
                p => p.DeletedAt == null && (status == null || p.Status == status)
            );

            // Get paginated data
            var allProofs = await _unitOfWork.PaymentProofs.SelectAllAsync(
                expression: p => p.DeletedAt == null && (status == null || p.Status == status),
                includes: new[] { "Order.Customer" }
            );

            var proofs = allProofs
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = _mapper.Map<List<PaymentProofResultDto>>(proofs);

            var pagination = new
            {
                page,
                page_size = pageSize,
                total_count = totalCount,
                total_pages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return SuccessResponse(new { proofs = result, pagination }, $"{totalCount} ta to'lov isboti");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllPaymentProofs xatolik");
            return ErrorResponse($"Xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// To'lov isbotlari statistikasi (Admin)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetPaymentProofStats()
    {
        try
        {
            var allProofs = await _unitOfWork.PaymentProofs.FindAsync(p => p.DeletedAt == null);

            var stats = new PaymentProofStatsDto
            {
                PendingCount = allProofs.Count(p => p.Status == PaymentStatus.UnderReview),
                PendingAmount = allProofs.Where(p => p.Status == PaymentStatus.UnderReview).Sum(p => p.Amount),
                VerifiedCount = allProofs.Count(p => p.Status == PaymentStatus.Verified),
                VerifiedAmount = allProofs.Where(p => p.Status == PaymentStatus.Verified).Sum(p => p.Amount),
                RejectedCount = allProofs.Count(p => p.Status == PaymentStatus.Rejected),
                RejectedAmount = allProofs.Where(p => p.Status == PaymentStatus.Rejected).Sum(p => p.Amount),
                TotalCount = allProofs.Count(),
                TotalAmount = allProofs.Sum(p => p.Amount)
            };

            return SuccessResponse(stats, "To'lov statistikasi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPaymentProofStats xatolik");
            return ErrorResponse($"Xatolik: {ex.Message}");
        }
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// To'lov isboti screenshot'ini saqlash
    /// </summary>
    private async Task<string> SavePaymentProofImage(IFormFile file, int orderId)
    {
        try
        {
            // Validate file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new Exception("Faqat JPG, PNG yoki PDF formatdagi fayllar qabul qilinadi");

            if (file.Length > 5 * 1024 * 1024) // 5 MB
                throw new Exception("Fayl hajmi 5 MB dan oshmasligi kerak");

            // Create directory if not exists
            var uploadPath = Path.Combine(_env.WebRootPath, "images", "payment_proofs");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var fileName = $"{orderId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            return $"images/payment_proofs/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SavePaymentProofImage xatolik");
            throw;
        }
    }
}
