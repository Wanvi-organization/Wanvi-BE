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
    public class NewsController : Controller
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
    }
}
