using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.MediaModelViews
{
    public class CreateMediaModel
    {
        [Required(ErrorMessage = "URL không được để trống.")]
        [Url(ErrorMessage = "URL không hợp lệ.")]
        public string Url { get; set; }

        [Required(ErrorMessage = "Loại phương tiện không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Loại phương tiện phải là số dương.")]
        public int Type { get; set; }

        public string? AltText { get; set; }
    }
}
