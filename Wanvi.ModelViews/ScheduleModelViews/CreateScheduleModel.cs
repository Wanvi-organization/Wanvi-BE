using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.ScheduleModelViews
{
    public class CreateScheduleModel
    {
        [Required(ErrorMessage = "Ngày không được để trống.")]
        public int Day { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống.")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian bắt đầu phải có định dạng HH:mm.")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc không được để trống.")]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Thời gian kết thúc phải có định dạng HH:mm.")]
        public string EndTime { get; set; }

        [Required(ErrorMessage = "Số lượng khách tối đa không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng khách tối đa phải là số dương.")]
        public int MaxTraveler { get; set; }
    }
}
