using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<IEnumerable<ResponseScheduleModel>> GetLocalGuideSchedulesAsync();
    }
}
