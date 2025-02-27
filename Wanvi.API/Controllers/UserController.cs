using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.UserModelViews;

namespace WanviBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Truy vấn danh sách hướng dẫn viên du lịch địa phương theo nhiều tiêu chí lọc và sắp xếp.
        /// </summary>
        /// <param name="latitude">Vĩ độ của vị trí cần tìm kiếm.</param>
        /// <param name="longitude">Kinh độ của vị trí cần tìm kiếm.</param>
        /// <param name="name">Tên hướng dẫn viên cần tìm (tùy chọn).</param>
        /// <param name="city">Tỉnh/thành phố của hướng dẫn viên (tùy chọn).</param>
        /// <param name="district">Quận/huyện của hướng dẫn viên (tùy chọn).</param>
        /// <param name="minPrice">Giá thuê tối thiểu theo giờ (tùy chọn).</param>
        /// <param name="maxPrice">Giá thuê tối đa theo giờ (tùy chọn).</param>
        /// <param name="minRating">Điểm đánh giá tối thiểu (tùy chọn).</param>
        /// <param name="maxRating">Điểm đánh giá tối đa (tùy chọn).</param>
        /// <param name="isVerified">Lọc hướng dẫn viên đã được xác minh (true) hoặc chưa xác minh (false) (tùy chọn).</param>
        /// <param name="sortByPrice">Sắp xếp theo giá (true - tăng dần, false - giảm dần, null - không áp dụng).</param>
        /// <param name="sortByNearest">Sắp xếp theo khoảng cách gần nhất (mặc định: true - gần nhất, false - không áp dụng).</param>
        [HttpGet("Get_Local_Guides")]
        public async Task<IActionResult> GetLocalGuides(double latitude, double longitude, string? name = null, string? city = null, string? district = null, double? minPrice = null, double? maxPrice = null, double? minRating = null, double? maxRating = null, bool? isVerified = null, bool? sortByPrice = null, bool? sortByNearest = null)
        {
            sortByNearest ??= true;
            var result = await _userService.GetLocalGuidesAsync(latitude, longitude, name, city, district, minPrice, maxPrice, minRating, maxRating, isVerified, sortByPrice, sortByNearest);
            return Ok(BaseResponse<IEnumerable<ResponseLocalGuideModel>>.OkResponse(result));
        }
        /// <summary>
        /// Lấy thông tin profile hướng dẫn viên bằng id.
        /// </summary>
        /// <param name="id">ID của hướng dẫn viên cần lấy</param>
        [HttpGet("Get_Local_Guide_Profile_Info_By_Id/{id}")]
        public async Task<IActionResult> GetLocalGuideProfileInfoById(Guid id)
        {
            return Ok(new BaseResponseModel<ResponseLocalGuideProfileModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _userService.GetLocalGuideProfileInfoByIdAsync(id)));
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

        [HttpPatch("Change_Password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            await _userService.ChangePassword(model);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: "Đổi mật khẩu thành công!"
             ));
        }
        [HttpPatch("Update_Profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileModel model)
        {
            await _userService.UpdateProfiel(model);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: "Cập nhật tài khoản thành công!"
             ));
        }
    }
}
