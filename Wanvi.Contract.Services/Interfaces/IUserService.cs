using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<NearbyResponseModelView>> GetNearbyLocalGuides(NearbyRequestModelView model);
    }
}
