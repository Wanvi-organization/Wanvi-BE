using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.ModelViews.UserModelViews;

namespace WanviBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Get_Nearby_Local_Guides")]
        public async Task<IActionResult> GetNearbyLocalGuides(NearbyRequestModelView model)
        {
            var result = await _userService.GetNearbyLocalGuides(model);
            return Ok(BaseResponse<IEnumerable<NearbyResponseModelView>>.OkResponse(result));
        }
    }
}
