using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.NewsDetailModelViews
{
    public class CreateNewsDetailModel
    {
        public string? MediaId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung chi tiết tin tức.")]
        public string Content { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thứ tự sắp xếp phải lớn hơn 0.")]
        public int SortOrder { get; set; }
    }
}
