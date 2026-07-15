using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public interface IBannerService
{
    Task<BannerListApiResponse> GetBannersAsync();
    Task<BannerDetailApiResponse> CreateBannerAsync(SaveBannerDto banner);
    Task<BannerDetailApiResponse> UpdateBannerAsync(int id, SaveBannerDto banner);
    Task<ApiResponse<object>> DeleteBannerAsync(int id);
}
