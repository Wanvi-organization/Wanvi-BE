using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.ActivityModelViews;
using Wanvi.ModelViews.TourModelViews;
using Wanvi.Services.Services;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;

        public TourController(ITourService tourService)
        {
            _tourService = tourService;
        }
        /// <summary>
        /// Lấy toàn bộ tour.
        /// </summary>
        [HttpGet("Get_All_Tours")]
        public async Task<IActionResult> GetAllTours()
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseTourModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _tourService.GetAllAsync()));
        }
        /// <summary>
        /// Lấy toàn bộ tour bằng id của hướng dẫn viên
        /// </summary>
        /// <param name="id">ID của hướng dẫn viên của tour cần lấy</param>
        [HttpGet("Get_All_Tours_By_Local_Guide_Id/{userId}")]
        public async Task<IActionResult> GetAllToursByLocalGuideId(string userId)
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseTourModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _tourService.GetAllByLocalGuideId(userId)));
        }
        /// <summary>
        /// Lấy tour bằng id.
        /// </summary>
        /// <param name="id">ID của tour cần lấy</param>
        [HttpGet("Get_Tour_By_Id/{id}")]
        public async Task<IActionResult> GetTourById(string id)
        {
            return Ok(new BaseResponseModel<ResponseTourModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _tourService.GetByIdAsync(id)));
        }
        /// <summary>
        /// Tạo một tour mới.
        /// </summary>
        /// <param name="model">Thông tin tour cần tạo</param>
        [HttpPost("Create_Tour")]
        public async Task<IActionResult> CreateTour(CreateTourModel model)
        {
            await _tourService.CreateAsync(model);
            return Ok(new BaseResponseModel<string?>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              message: "Tạo mới tour thành công."));
        }
        /// <summary>
        /// Cập nhật một tour.
        /// </summary>
        /// <param name="id">ID của tour cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật cho tour</param>
        [HttpPatch("Update_Tour/{id}")]
        public async Task<IActionResult> UpdateTour(string id, UpdateTourModel model)
        {
            await _tourService.UpdateAsync(id, model);
            return Ok(new BaseResponseModel<string?>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               message: "Cập nhật tour thành công."));
        }
        /// <summary>
        /// Xóa một tour.
        /// </summary>
        /// <param name="id">id của tour cần xóa.</param>
        ///
        [HttpDelete("Delete_Tour/{id}")]
        public async Task<IActionResult> DeleteTour(string id)
        {
            await _tourService.DeleteAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa tour thành công."));
        }
    }
}
