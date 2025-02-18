using Wanvi.ModelViews.DistrictModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IDistrictService
    {
        Task<IEnumerable<ResponseDistrictModel>> GetAllByCityId(string icityId);
    }
}
