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
        /// Lấy danh sách tất cả người dùng trong hệ thống, có thể lọc theo vai trò và thành phố.
        /// </summary>
        /// <param name="roleId">ID của vai trò để lọc danh sách người dùng (tùy chọn).</param>
        /// <param name="cityId">ID của thành phố để lọc danh sách người dùng (tùy chọn).</param>
        [HttpGet("get_all_users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] Guid? roleId, [FromQuery] string? cityId)
        {
            var users = await _userService.GetAllAsync(roleId, cityId);

            return Ok(new BaseResponseModel<IEnumerable<AdminResponseUserModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: users
            ));
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
        /// <summary>
        /// Admin mở khóa tour của HDV
        /// </summary>
        [HttpPatch("Unlock_Booking_Of_TourGuide")]
        public async Task<IActionResult> UnlockBookingOfTourGuide(UnlockBookingOfTourGuideModel model)
        {
            var res = await _userService.UnlockBookingOfTourGuide(model);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        [HttpPost("assign_role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignUserRoleModel model)
        {
            await _userService.AssignUserToRoleAsync(model.UserId, model.RoleId);

            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: "Gán vai trò cho người dùng thành công!"
            ));
        }

        /// <summary>
        /// Admin cập nhật hồ sơ cho Traveler (hỗ trợ PATCH với tất cả trường optional)
        /// </summary>
        [HttpPatch("update_traveler_profile/{userId}")]
        public async Task<IActionResult> UpdateTravelerProfile(Guid userId, [FromBody] UpdateTravelerProfileModel model)
        {
            await _userService.UpdateTravelerProfileAsync(userId, model);

            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: "Cập nhật hồ sơ Traveler thành công!"
            ));
        }

        /// <summary>
        /// Admin cập nhật hồ sơ cho LocalGuide (hỗ trợ PATCH với tất cả trường optional)
        /// </summary>
        [HttpPatch("update_localguide_profile/{userId}")]
        public async Task<IActionResult> UpdateLocalGuideProfile(Guid userId, [FromBody] UpdateLocalGuideProfileModel model)
        {
            await _userService.UpdateLocalGuideProfileAsync(userId, model);

            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: "Cập nhật hồ sơ LocalGuide thành công!"
            ));
        }
    }
}
