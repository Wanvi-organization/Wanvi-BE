using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.HashtagModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HashtagController : ControllerBase
    {
        private readonly IHashtagService _hashtagService;

        public HashtagController(IHashtagService hashtagService)
        {
            _hashtagService = hashtagService;
        }
        /// <summary>
        /// Lấy toàn bộ hashtag.
        /// </summary>
        [HttpGet("Get_All_Hashtags")]
        public async Task<IActionResult> GetAllHashtags()
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseHashtagModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _hashtagService.GetAllAsync()));
        }
        /// <summary>
        /// Lấy hashtag bằng id.
        /// </summary>
        /// <param name="id">ID của hashtag cần lấy</param>
        [HttpGet("Get_Hashtag_By_Id/{id}")]
        public async Task<IActionResult> GetHashtagById(string id)
        {
            return Ok(new BaseResponseModel<ResponseHashtagModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _hashtagService.GetByIdAsync(id)));
        }
        /// <summary>
        /// Tạo một hashtag mới.
        /// </summary>
        /// <param name="model">Thông tin hashtag cần tạo</param>
        [HttpPost("Create_Hashtag")]
        public async Task<IActionResult> CreateHashtag(CreateHashtagModel model)
        {
            await _hashtagService.CreateAsync(model);
            return Ok(new BaseResponseModel<string?>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              message: "Tạo mới hashtag thành công."));
        }
        /// <summary>
        /// Cập nhật một hashtag.
        /// </summary>
        /// <param name="id">ID của hashtag cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật cho hashtag</param>
        [HttpPatch("Update_Hashtag/{id}")]
        public async Task<IActionResult> UpdateHashtag(string id, UpdateHashtagModel model)
        {
            await _hashtagService.UpdateAsync(id, model);
            return Ok(new BaseResponseModel<string?>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               message: "Cập nhật hashtag thành công."));
        }
        /// <summary>
        /// Xóa một hashtag.
        /// </summary>
        /// <param name="id">id của hashtag cần xóa.</param>
        [HttpDelete("Delete_Hashtag/{id}")]
        public async Task<IActionResult> DeleteHashtag(string id)
        {
            await _hashtagService.DeleteAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa hashtag thành công."));
        }
    }
}
