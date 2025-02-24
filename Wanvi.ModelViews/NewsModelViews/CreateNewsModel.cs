using System.ComponentModel.DataAnnotations;
using Wanvi.ModelViews.NewsDetailModelViews;

namespace Wanvi.ModelViews.NewsModelViews
{
    public class CreateNewsModel
    {
        [Required(ErrorMessage = "Vui lòng điền tiêu đề.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng điền tóm tắt.")]
        public string Summary { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public string CategoryId { get; set; }
        [Required(ErrorMessage = "Vui lòng thêm ít nhất một chi tiết tin tức.")]
        [MinLength(1, ErrorMessage = "Vui lòng thêm ít nhất một chi tiết tin tức.")]
        public List<CreateNewsDetailModel> NewsDetails { get; set; } =  new();
    }
}