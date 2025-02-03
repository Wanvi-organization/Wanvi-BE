using Wanvi.ModelViews.DistrictModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IDistrictService
    {
        IEnumerable<ResponseDistrictModel> GetByCityId(string id);
    }
}
