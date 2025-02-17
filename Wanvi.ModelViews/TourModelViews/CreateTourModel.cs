using System.ComponentModel.DataAnnotations;
using Wanvi.ModelViews.AddressModelViews;
using Wanvi.ModelViews.MediaModelViews;
using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.ModelViews.TourModelViews
{
    public class CreateTourModel
    {
        [Required(ErrorMessage = "Tên tour không được để trống.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả tour không được để trống.")]
        public string Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá tour theo giờ phải là số dương.")]
        public double HourlyRate { get; set; }

        [Required(ErrorMessage = "Địa chỉ đón khách không được để trống.")]
        public CreateAddressModel PickupAddress { get; set; }

        [Required(ErrorMessage = "Địa chỉ trả khách không được để trống.")]
        public CreateAddressModel DropoffAddress { get; set; }

        [Required(ErrorMessage = "Danh sách địa điểm tour không được để trống.")]
        [MinLength(1, ErrorMessage = "Danh sách địa điểm tour cần ít nhất một địa điểm.")]
        public List<CreateAddressModel> TourAddresses { get; set; } = new();

        [Required(ErrorMessage = "Lịch trình không được để trống.")]
        [MinLength(1, ErrorMessage = "Lịch trình cần ít nhất một ngày.")]
        public List<CreateScheduleModel> Schedules { get; set; } = new();

        [Required(ErrorMessage = "Tour cần ít nhất một ảnh/video.")]
        [MinLength(1, ErrorMessage = "Tour cần ít nhất một ảnh/video.")]
        public List<CreateMediaModel> Medias { get; set; } = new();

        [Required(ErrorMessage = "Hoạt động của tour không được để trống.")]
        [MinLength(1, ErrorMessage = "Tour phải có ít nhất một hoạt động.")]
        public List<string> TourActivityIds { get; set; } = new();
    }
}
