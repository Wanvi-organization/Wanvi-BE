using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.AuthModelViews
{
    public class TokenModelView
    {
        [Required(ErrorMessage = "Token không được để trống")]
        public string Token { get; set; }
    }
}
