using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.CommentModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Lấy tất cả các bình luận của tin tức theo ID tin tức.
        /// </summary>
        /// <param name="newsId">ID của tin tức</param>
        [HttpGet("Get_All_Comments_By_NewsId/{newsId}")]
        public async Task<IActionResult> GetAllCommentsByNewsId(string newsId)
        {
            var comments = await _commentService.GetAllCommentsByNewsIdAsync(newsId);
            return Ok(new BaseResponseModel<IEnumerable<ResponseCommentModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: comments));
        }

        /// <summary>
        /// Tạo một bình luận cho tin tức.
        /// </summary>
        /// <param name="newsId">ID của tin tức</param>
        /// <param name="model">Thông tin bình luận cần tạo</param>
        [HttpPost("Create_News_Comment/{newsId}")]
        public async Task<IActionResult> CreateNewsComment(string newsId, CreateCommentModel model)
        {
            await _commentService.CreateNewsCommentAsync(newsId, model);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Tạo mới bình luận cho tin tức thành công."));
        }

        /// <summary>
        /// Trả lời một bình luận.
        /// </summary>
        /// <param name="commentId">ID của bình luận cần trả lời</param>
        /// <param name="model">Thông tin trả lời bình luận</param>
        [HttpPost("Reply_Comment/{commentId}")]
        public async Task<IActionResult> ReplyComment(string commentId, CreateCommentModel model)
        {
            await _commentService.ReplyCommentAsync(commentId, model);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Trả lời bình luận thành công."));
        }

        /// <summary>
        /// Người dùng like/unlike bình luận.
        /// </summary>
        /// <param name="commentId">ID của bình luận</param>
        [HttpPatch("Like_Or_Unlike_Comment/{commentId}")]
        public async Task<IActionResult> LikeOrUnlikeComment(string commentId)
        {
            var isLiked = await _commentService.LikeUnlikeCommentAsync(commentId);

            if (isLiked)
            {
                return Ok(new BaseResponseModel<string?>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    message: "Bạn đã thích bình luận này."));
            }
            else
            {
                return Ok(new BaseResponseModel<string?>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    message: "Bạn đã bỏ thích bình luận này."));
            }
        }

        /// <summary>
        /// Cập nhật bình luận.
        /// </summary>
        /// <param name="commentId">ID của bình luận cần cập nhật</param>
        /// <param name="model">Thông tin bình luận cần cập nhật</param>
        [HttpPatch("Update_Comment/{commentId}")]
        public async Task<IActionResult> UpdateComment(string commentId, UpdateCommentModel model)
        {
            await _commentService.UpdateCommentAsync(commentId, model);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Cập nhật bình luận thành công."));
        }

        /// <summary>
        /// Xóa bình luận.
        /// </summary>
        /// <param name="commentId">ID của bình luận cần xóa</param>
        [HttpDelete("Delete_Comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            await _commentService.DeleteCommentAsync(commentId);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa bình luận thành công."));
        }
    }
}
