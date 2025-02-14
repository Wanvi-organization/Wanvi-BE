using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
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

        [HttpGet("Get_All_Activities")]
        public IActionResult GetAllActivities()
        {
            var result = _activityService.GetAll();
            return Ok(BaseResponse<IEnumerable<ResponseActivityModel>>.OkResponse(result));
        }
    }
}
