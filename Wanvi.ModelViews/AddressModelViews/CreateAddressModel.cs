using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.AddressModelViews
{
    public class CreateAddressModel
    {
        [Required(ErrorMessage = "Vĩ độ không được để trống.")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ không được để trống.")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng từ -180 đến 180.")]
        public double Longitude { get; set; }
    }
}
