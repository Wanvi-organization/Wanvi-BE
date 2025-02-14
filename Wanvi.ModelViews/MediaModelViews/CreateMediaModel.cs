using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.MediaModelViews
{
    public class CreateMediaModel
    {
        [Required]
        public string Url { get; set; }

        [Required]
        public int Type { get; set; }
        public string? AltText { get; set; }
    }
}
