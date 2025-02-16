using Wanvi.ModelViews.DashboardModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<ResponseDashboardModel> GetDashboardDataAsync();
    }
}
