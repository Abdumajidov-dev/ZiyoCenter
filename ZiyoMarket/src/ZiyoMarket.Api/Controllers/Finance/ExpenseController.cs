using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Expenses;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Finance;

[ApiController]
[Route("api/expenses")]
[Authorize]
public class ExpenseController : BaseController
{
    private readonly IExpenseService _expenseService;

    public ExpenseController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    /// <summary>
    /// Xarajatlar ro'yxati (sana oralig'i va kategoriya bo'yicha filter)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ExpenseCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 20)
    {
        var result = await _expenseService.GetPagedAsync(from, to, category, page, page_size);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Xarajat summasi (hisobot uchun)
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddMonths(-1);
        var toDate = to ?? DateTime.Today;
        var result = await _expenseService.GetSummaryAsync(fromDate, toDate);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Bitta xarajat
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _expenseService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Yangi xarajat qo'shish
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _expenseService.CreateAsync(dto, GetCurrentUserId());
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return StatusCode(201, new { success = true, data = result.Data });
    }

    /// <summary>
    /// Xarajatni yangilash
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto)
    {
        var result = await _expenseService.UpdateAsync(id, dto, GetCurrentUserId());
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Xarajatni o'chirish
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _expenseService.DeleteAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }
}
