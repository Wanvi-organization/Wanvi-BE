using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.UserModelViews
{
    public class NearbyRequestModelView
    {
        [Required(ErrorMessage = "Vĩ độ là bắt buộc.")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ là bắt buộc.")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double Longitude { get; set; }
    }
}
