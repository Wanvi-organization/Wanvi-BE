using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.AuthModelViews;
using Wanvi.ModelViews.BookingModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        /// <summary>
        /// lấy danh sách booking của user
        /// </summary>
        [HttpGet("get_booking_user")]
        public async Task<IActionResult> GetBookingUser(string? searchNote = null,
            string? sortBy = null,
            bool isAscending = false,
            string? status = null)
        {
            List<GetBookingUserModel> res = await _bookingService.GetBookingUser(searchNote, sortBy, isAscending, status);
            return Ok(new BaseResponseModel<List<GetBookingUserModel>>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// lấy booking detail của user
        /// </summary>
        [HttpGet("get_booking_detail_user")]
        public async Task<IActionResult> GetBookingUser(string bookingId)
        {
            var res = await _bookingService.GetBookingDetailUser(bookingId);
            return Ok(new BaseResponseModel<GetBookingDetailUserModel>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        /// <summary>
        /// lấy danh sách booking của admnin
        /// </summary>
        [HttpGet("get_booking_admin")]
        public async Task<IActionResult> GetBookingAdmin(string? searchNote = null,
            string? sortBy = null,
            bool isAscending = false,
            string? status = null)
        {
            List<GetBookingUserModel> res = await _bookingService.GetBookingAdmin(searchNote, sortBy, isAscending, status);
            return Ok(new BaseResponseModel<List<GetBookingUserModel>>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        /// <summary>
        /// lấy danh sách booking của tour guide màn 1
        /// </summary>
        [HttpGet("get_booking_tour_guide")]
        public async Task<IActionResult> GetBookingTourGuide(
            string? rentalDate = null,
            string? status = null,
            string? scheduleId = null,
            int? minTravelers = null,
            int? maxTravelers = null)
        {
            List<GetBookingUserDetailModel> res = await _bookingService.GetBookingsByTourGuide(rentalDate, status, scheduleId, minTravelers, maxTravelers);
            return Ok(new BaseResponseModel<List<GetBookingUserDetailModel>>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// lấy danh sách booking của tour guide màn 2
        /// </summary>
        [HttpGet("Get_Booking_Details_By_Schedule")]
        public async Task<IActionResult> GetBookingDetailsBySchedule(string scheduleId, string rentalDate,
            string? status = null,
            int? minPrice = null,
            int? maxPrice = null,
            string sortBy = "CustomerName",
            bool ascending = true)
        {
            var res = await _bookingService.GetBookingSummaryBySchedule(scheduleId, rentalDate, status, minPrice, maxPrice, sortBy, ascending);
            return Ok(new BaseResponseModel<GetBookingGuideModel>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// lấy danh sách booking của tour guide màn 3
        /// </summary>
        [HttpGet("Get_Booking_Details_By_Id")]
        public async Task<IActionResult> GetBookingDetailsById(string bookingId)
        {
            var res = await _bookingService.GetBookingDetailsById(bookingId);
            return Ok(new BaseResponseModel<GetBookingGuideScreen3Model>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Tạo booking 100%
        /// </summary>
        [HttpPost("create_booking")]
        public async Task<IActionResult> CreateBooingAll(CreateBookingModel request)
        {
            string res = await _bookingService.CreateBookingAll(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Tạo booking cọc 50%
        /// </summary>
        [HttpPost("create_booking_haft")]
        public async Task<IActionResult> CreateBookingHaft(CreateBookingModel request)
        {
            string res = await _bookingService.CreateBookingHaft(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Chuyển tiền từ cọc sang cho HDV
        /// </summary>
        [HttpPatch("change_booking_to_user")]
        public async Task<IActionResult> ChangeBookingToUser(ChangeBookingToUserModel request)
        {
            string res = await _bookingService.ChangeBookingToUser(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Gửi yêu cầu rút tiền về ngân hàng cho HDV
        /// </summary>
        [HttpPost("Withdraw_Money_From_Booking")]
        public async Task<IActionResult> WithdrawMoneyFromBooking(WithdrawMoneyFromBookingModel request)
        {
            string res = await _bookingService.WithdrawMoneyFromBooking(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        /// <summary>
        /// Hủy đơn dành cho hướng dẫn viên
        /// </summary>
        [HttpPatch("Cancel_Booking_For_Guide")]
        public async Task<IActionResult> CancelBookingForGuide(CancelBookingForGuideModel request)
        {
            string res = await _bookingService.CancelBookingForGuide(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Hướng dẫn viên hoàn thành tour
        /// </summary>
        [HttpPatch("Complete_Booking_For_TourGuide")]
        public async Task<IActionResult> CompleteBookingForTourGuide(CompleteBookingForTourGuideModel request)
        {
            string res = await _bookingService.CompleteBookingForTourGuide(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Hủy đơn dành cho khách hàng
        /// </summary>
        [HttpPatch("Cancel_Booking_For_Customer")]
        public async Task<IActionResult> CancelBookingForCustomer(CancelBookingForCustomerModel request)
        {
            string res = await _bookingService.CancelBookingForCustomer(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Hủy đơn chưa trả tiền cọc dành cho khách hàng
        /// </summary>
        [HttpPatch("Cancel_Booking_No_Deposit_For_Customer")]
        public async Task<IActionResult> CancelBookingNoDepositForCustomer(CancelBookingForCustomerModel request)
        {
            string res = await _bookingService.CancelBookingNoDepositForCustomer(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Admin hủy tât các các tour của HDV vi phạm quy định của app
        /// </summary>
        [HttpPatch("Cancel_Booking_For_Admin")]
        public async Task<IActionResult> CancelBookingForAdmin(CancelBookingForAdminModel request)
        {
            string res = await _bookingService.CancelBookingForAdmin(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
    }
}
