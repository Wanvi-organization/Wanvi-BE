using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.AuthModelViews
{
    public class CheckGoogleModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ (Family Name) là bắt buộc.")]
        public string FamilyName { get; set; }

        [Required(ErrorMessage = "Tên (Given Name) là bắt buộc.")]
        public string GivenName { get; set; }

        [Required(ErrorMessage = "ID là bắt buộc.")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Ảnh đại diện là bắt buộc.")]
        public string Photo { get; set; }
    }
}
