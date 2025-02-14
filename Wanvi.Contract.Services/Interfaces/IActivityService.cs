using Wanvi.ModelViews.ActivityModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IActivityService
    {
        IEnumerable<ResponseActivityModel> GetAll();
    }
}
