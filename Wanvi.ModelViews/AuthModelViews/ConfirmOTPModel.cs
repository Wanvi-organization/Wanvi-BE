using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.AuthModelViews
{
    public class ConfirmOTPModel
    {
        [Required(ErrorMessage = "OTP bắt buộc")]
        [MaxLength(6, ErrorMessage = "OTP không hợp lệ")]
        [MinLength(0, ErrorMessage = "OTP không hợp lệ")]
        [StringLength(6, ErrorMessage = "OTP không hợp lệ")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP không hợp lệ")]
        public string OTP { get; set; }
    }
}
