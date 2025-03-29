using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.ScheduleModelViews
{
    public class UpdateScheduleModel
    {
        public int? Day { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int? MaxTraveler { get; set; }
    }
}
