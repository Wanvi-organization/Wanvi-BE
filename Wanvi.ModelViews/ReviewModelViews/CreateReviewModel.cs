using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.ReviewModelViews
{
    public class CreateReviewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn đánh giá sao.")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải nằm trong khoảng từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống.")]
        [StringLength(1000, ErrorMessage = "Nội dung không được dài quá 1000 ký tự.")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một phương tiện.")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một phương tiện.")]
        [MaxLength(5, ErrorMessage = "Không thể chọn quá 5 phương tiện.")]
        public List<string> MediaIds { get; set; }
    }
}
