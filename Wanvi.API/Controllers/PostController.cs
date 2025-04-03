using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.PostModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }
        /// <summary>
        /// Lấy toàn bộ bài đăng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponsePostModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _postService.GetAllPostsAsync()));
        }
        /// <summary>
        /// Lấy toàn bộ bài đăng của người dùng.
        /// </summary>
        [HttpGet("get_posts_by_userId/{userId}")]
        public async Task<IActionResult> GetPostsByUserId(Guid userId)
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponsePostModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _postService.GetPostsByUserIdAsync(userId)));
        }
        /// <summary>
        /// Lấy bài đăng bằng ID.
        /// </summary>
        /// <param name="id">ID của bài đăng cần lấy</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(string id)
        {
            return Ok(new BaseResponseModel<ResponsePostModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _postService.GetPostByIdAsync(id)));
        }
        /// <summary>
        /// Tạo một bài đăng mới.
        /// </summary>
        /// <param name="model">Thông tin bài đăng cần tạo</param>
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostModel model)
        {
            await _postService.CreatePostAsync(model);
            return Ok(new BaseResponseModel<string?>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              message: "Tạo mới bài đăng thành công."));
        }
        /// <summary>
        /// Cập nhật một bài đăng.
        /// </summary>
        /// <param name="id">ID của bài đăng cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật cho bài đăng</param>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] UpdatePostModel model)
        {
            await _postService.UpdatePostAsync(id, model);
            return Ok(new BaseResponseModel<string?>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               message: "Cập nhật bài đăng thành công."));
        }
        /// <summary>
        /// Xóa một bài đăng.
        /// </summary>
        /// <param name="id">ID của bài đăng cần xóa.</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            await _postService.DeletePostAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa bài đăng thành công."));
        }
        /// <summary>
        /// Thích một bài đăng.
        /// </summary>
        /// <param name="id">ID của bài đăng cần thích</param>
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(string id)
        {
            await _postService.LikePostAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Đã thích bài đăng."));
        }
        /// <summary>
        /// Bỏ thích một bài đăng.
        /// </summary>
        /// <param name="id">ID của bài đăng cần bỏ thích</param>
        [HttpPost("{id}/unlike")]
        public async Task<IActionResult> UnlikePost(string id)
        {
            await _postService.UnlikePostAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Đã bỏ thích bài đăng."));
        }
    }
}
