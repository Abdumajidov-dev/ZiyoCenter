using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface INewsService
{
    /// <param name="includeUnpublished">Admin uchun qoralamalar ham</param>
    Task<Result<List<NewsDto>>> GetAllAsync(bool includeUnpublished);
    Task<Result<NewsDto>> GetByIdAsync(int id, bool includeUnpublished);
    Task<Result<NewsDto>> CreateAsync(SaveNewsDto dto, int createdBy);
    Task<Result<NewsDto>> UpdateAsync(int id, SaveNewsDto dto, int updatedBy);
    Task<Result> DeleteAsync(int id, int deletedBy);
}

public interface IFaqService
{
    /// <param name="includeInactive">Admin uchun yashirilganlar ham</param>
    Task<Result<List<FaqDto>>> GetAllAsync(bool includeInactive);
    Task<Result<FaqDto>> GetByIdAsync(int id);
    Task<Result<FaqDto>> CreateAsync(SaveFaqDto dto, int createdBy);
    Task<Result<FaqDto>> UpdateAsync(int id, SaveFaqDto dto, int updatedBy);
    Task<Result> DeleteAsync(int id, int deletedBy);
}
