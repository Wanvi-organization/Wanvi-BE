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

        [HttpGet("Get_Local_Guides")]
        public async Task<IActionResult> GetLocalGuides(double latitude, double longitude, string? name = null, string? city = null, string? district = null, double? minPrice = null, double? maxPrice = null, double? minRating = null, double? maxRating = null, bool? isVerified = null, bool? sortByPriceAsc = null, bool? sortByPriceDesc = null, bool? sortByNearest = null)
        {
            var result = await _userService.GetLocalGuidesAsync(latitude, longitude, name, city, district, minPrice, maxPrice, minRating, maxRating, isVerified, sortByPriceAsc, sortByPriceDesc, sortByNearest);
            return Ok(BaseResponse<IEnumerable<ResponseLocalGuideModel>>.OkResponse(result));
        }
    }
}
