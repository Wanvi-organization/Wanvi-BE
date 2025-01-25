using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.UserModelViews;
using Wanvi.Services.Services;

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

        [HttpGet("Get_Infor")]
        public async Task<IActionResult> GetInfor()
        {
            UserInforModel res = await _userService.GetInfor();
            return Ok(new BaseResponseModel<UserInforModel>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        [HttpGet("Get_Traveler_Base_Id")]
        public async Task<IActionResult> GetInforTravelerBaseId(Guid Id)
        {
            UserInforModel res = await _userService.GetTravelerBaseId(Id);
            return Ok(new BaseResponseModel<UserInforModel>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        [HttpPost("Change_Password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            await _userService.ChangePassword(model);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: "Đổi mật khẩu thành công"
             ));
        }
    }
}
