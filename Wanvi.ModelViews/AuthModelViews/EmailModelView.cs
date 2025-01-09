using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.AuthModelViews
{
    public class EmailModelView
    {
        [Required(ErrorMessage = "Email bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
    }
}
