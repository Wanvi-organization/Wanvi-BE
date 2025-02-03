using Wanvi.ModelViews.CityModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ICityService
    {
        IEnumerable<ResponseCityModel> GetAll();
    }
}
