using Wanvi.ModelViews.CityModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ICityService
    {
        Task<IEnumerable<ResponseCityModel>> GetAllAsync();
    }
}
