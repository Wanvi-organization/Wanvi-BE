using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.ActivityModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }
        /// <summary>
        /// Lấy toàn bộ hoạt động.
        /// </summary>
        [HttpGet("Get_All_Activities")]
        public async Task<IActionResult> GetAllActivities()
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseActivityModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _activityService.GetAllAsync()));
        }
        /// <summary>
        /// Lấy hoạt động bằng id.
        /// </summary>
        /// <param name="id">ID của hoạt động cần lấy</param>
        [HttpGet("Get_Activity_By_Id/{id}")]
        public async Task<IActionResult> GetActivityById(string id)
        {
            return Ok(new BaseResponseModel<ResponseActivityModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _activityService.GetByIdAsync(id)));
        }
        /// <summary>
        /// Tạo một hoạt động mới.
        /// </summary>
        /// <param name="model">Thông tin hoạt động cần tạo</param>
        [HttpPost("Create_Activity")]
        public async Task<IActionResult> CreateActivity(CreateActivityModel model)
        {
            await _activityService.CreateAsync(model);
            return Ok(new BaseResponseModel<string?>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              message: "Tạo mới hoạt động thành công."));
        }
        /// <summary>
        /// Cập nhật một hoạt động.
        /// </summary>
        /// <param name="id">ID của hoạt động cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật cho hoạt động</param>
        [HttpPatch("Update_Activity/{id}")]
        public async Task<IActionResult> UpdateActivity(string id, UpdateActivityModel model)
        {
            await _activityService.UpdateAsync(id, model);
            return Ok(new BaseResponseModel<string?>(
               statusCode: StatusCodes.Status200OK,
               code: ResponseCodeConstants.SUCCESS,
               message: "Cập nhật hoạt động thành công."));
        }
        /// <summary>
        /// Xóa một hoạt động.
        /// </summary>
        /// <param name="id">id của hoạt động cần xóa.</param>
        ///
        [HttpDelete("Delete_Activity/{id}")]
        public async Task<IActionResult> DeleteActivity(string id)
        {
            await _activityService.DeleteAsync(id);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa hoạt động thành công."));
        }
    }
}
