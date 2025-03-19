using System.ComponentModel.DataAnnotations;

namespace Wanvi.ModelViews.CommentModelViews
{
    public class UpdateCommentModel
    {
        [Required(ErrorMessage = "Nội dung bình luận là bắt buộc.")]
        [StringLength(500, ErrorMessage = "Nội dung bình luận không được vượt quá 500 ký tự.")]
        public string Content { get; set; }
    }
}
