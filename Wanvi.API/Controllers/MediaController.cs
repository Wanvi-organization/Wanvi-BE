using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.MediaModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/media")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        /// <summary>
        /// Upload nhiều ảnh lên server và lưu đường dẫn vào database.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadMedia([FromForm] UploadMediaModel model)
        {
            var result = await _mediaService.UploadAsync(model);
            return Ok(new BaseResponseModel<IEnumerable<ResponseMediaModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: result));
        }

        /// <summary>
        /// Xóa một ảnh trên server và trong database.
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMedia(string id)
        {
            await _mediaService.DeleteAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa ảnh thành công."));
        }
    }
}
