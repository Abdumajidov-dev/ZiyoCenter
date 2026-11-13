using ZiyoMarket.Service.DTOs.Products;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// Mijozning qiziqishlariga qarab mahsulot tavsiya qilish.
/// </summary>
public interface IRecommendationService
{
    /// <summary>Mijoz mahsulot sahifasini ochdi.</summary>
    Task TrackViewAsync(int customerId, int productId);

    /// <summary>Mijoz qidiruvda so'z yozdi.</summary>
    Task TrackSearchAsync(int customerId, string term);

    /// <summary>
    /// "Siz uchun": mijoz ko'rgan, yoqtirgan, savatga qo'shgan, sotib olgan va qidirgan
    /// narsalariga o'xshash mahsulotlar. Mijoz noma'lum bo'lsa, eng ommabop mahsulotlar.
    /// </summary>
    Task<List<ProductListDto>> GetRecommendedAsync(int? customerId, int count = 20);

    /// <summary>Mahsulot sahifasidagi "O'xshash mahsulotlar".</summary>
    Task<List<ProductListDto>> GetSimilarAsync(int productId, int? customerId, int count = 12);
}
