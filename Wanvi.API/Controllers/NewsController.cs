using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.ActivityModelViews;
using Wanvi.ModelViews.NewsModelViews;
using Wanvi.Services.Services;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }
        /// <summary>
        /// Lấy toàn bộ tin tức.
        /// </summary>
        [HttpGet("Get_All_News")]
        public async Task<IActionResult> GetAllNews()
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseNewsModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _newsService.GetAllAsync()));
        }
        /// <summary>
        /// Lấy toàn bộ tin tức bằng id danh mục.
        /// </summary>
        [HttpGet("Get_All_News_By_Category_Id")]
        public async Task<IActionResult> GetAllNewsByCategoryId(string id)
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseNewsModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _newsService.GetAllByCategoryIdAsync(id)));
        }
        /// <summary>
        /// Lấy tin tức bằng id.
        /// </summary>
        /// <param name="id">ID của tin tức cần lấy</param>
        [HttpGet("Get_News_By_Id/{id}")]
        public async Task<IActionResult> GetNewsById(string id)
        {
            return Ok(new BaseResponseModel<ResponseNewsModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _newsService.GetByIdAsync(id)));
        }
        /// <summary>
        /// Tạo một tin tức mới.
        /// </summary>
        /// <param name="model">Thông tin tin tức cần tạo</param>
        [HttpPost("Create_News")]
        public async Task<IActionResult> CreateNews(CreateNewsModel model)
        {
            await _newsService.CreateAsync(model);
            return Ok(new BaseResponseModel<string?>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              message: "Tạo mới tin tức thành công."));
        }
        /// <summary>
        /// Cập nhật một tin tức.
        /// </summary>
        /// <param name="id">ID của tin tức cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật cho tin tức</param>
        [HttpPatch("Update_News/{id}")]
        public async Task<IActionResult> UpdateNews(string id, UpdateNewsModel model)
        {
            await _newsService.UpdateAsync(id, model);
            return Ok(new BaseResponseModel<string?>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               message: "Cập nhật tin tức thành công."));
        }
        /// <summary>
        /// Xóa một tin tức.
        /// </summary>
        /// <param name="id">id của tin tức cần xóa.</param>
        ///
        [HttpDelete("Delete_News/{id}")]
        public async Task<IActionResult> DeleteNews(string id)
        {
            await _newsService.DeleteAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa tin tức thành công."));
        }
    }
}
