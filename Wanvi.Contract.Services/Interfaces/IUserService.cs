using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<ResponseLocalGuideModel>> GetLocalGuidesAsync(double latitude, double longitude, string? name = null, string ? city = null, string? district = null, double? minPrice = null, double? maxPrice = null, double? minRating = null, double? maxRating = null, bool? isVerified = null, bool? sortByPrice = null, bool? sortByNearest = null);
    }
}
