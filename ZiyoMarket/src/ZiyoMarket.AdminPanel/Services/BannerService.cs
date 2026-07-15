using ZiyoMarket.AdminPanel.Models;

namespace ZiyoMarket.AdminPanel.Services;

public class BannerService : IBannerService
{
    private readonly IApiService _apiService;

    public BannerService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<BannerListApiResponse> GetBannersAsync()
    {
        var result = await _apiService.GetAsync<BannerListApiResponse>("content/type/Banner");
        return result.Data ?? new BannerListApiResponse { Success = false, Message = result.Message };
    }

    public async Task<BannerDetailApiResponse> CreateBannerAsync(SaveBannerDto banner)
    {
        var result = await _apiService.PostAsync<BannerDetailApiResponse>("content", banner);
        return result.Data ?? new BannerDetailApiResponse { Success = false, Message = result.Message };
    }

    public async Task<BannerDetailApiResponse> UpdateBannerAsync(int id, SaveBannerDto banner)
    {
        var result = await _apiService.PutAsync<BannerDetailApiResponse>($"content/{id}", banner);
        return result.Data ?? new BannerDetailApiResponse { Success = false, Message = result.Message };
    }

    public async Task<ApiResponse<object>> DeleteBannerAsync(int id)
    {
        return await _apiService.DeleteAsync<object>($"content/{id}");
    }
}
