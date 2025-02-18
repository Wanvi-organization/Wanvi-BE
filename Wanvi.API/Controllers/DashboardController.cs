using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.ModelViews.DashboardModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("Get_All_Dashboard_Data")]
        public async Task<IActionResult> GetAllDashboardData()
        {
            var result = await _dashboardService.GetDashboardDataAsync();
            return Ok(BaseResponse<ResponseDashboardModel>.OkResponse(result));
        }
    }
}
