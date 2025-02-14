using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.ScheduleModelViews
{
    public class CreateScheduleModel
    {
        [Required]
        public int Day { get; set; }
        [Required]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian phải có định dạng HH:mm.")]
        public string StartTime { get; set; }

        [Required]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian phải có định dạng HH:mm.")]
        public string EndTime { get; set; }
        [Required]
        public int MaxTraveler { get; set; }
    }
}
