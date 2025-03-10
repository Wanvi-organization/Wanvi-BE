using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// Lấy danh sách lịch trình của hướng dẫn viên địa phương.
        /// </summary>
        [HttpGet("Get_LocalGuide_Schedules")]
        public async Task<IActionResult> GetLocalGuideSchedules()
        {
            var schedules = await _scheduleService.GetLocalGuideSchedulesAsync();
            return Ok(new BaseResponseModel<IEnumerable<ResponseScheduleModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: schedules));
        }
    }

}
