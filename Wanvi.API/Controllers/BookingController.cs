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
            List<GetBookingUsermodel> res = await _bookingService.GetBookingUser(searchNote, sortBy, isAscending, status);
            return Ok(new BaseResponseModel<List<GetBookingUsermodel>>(
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
            List<GetBookingUsermodel> res = await _bookingService.GetBookingAdmin(searchNote, sortBy, isAscending, status);
            return Ok(new BaseResponseModel<List<GetBookingUsermodel>>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        /// <summary>
        /// lấy danh sách booking của tour guide
        /// </summary>
        [HttpGet("get_booking_tour_guide")]
        public async Task<IActionResult> GetBookingTourGuide(string? rentalDate = null,
            string? status = null,
            string? scheduleId = null,
            int? minTravelers = null,
            int? maxTravelers = null,
            string? sortBy = "RentalDate",
            bool ascending = false)
        {
            List<GetBookingGuideModel> res = await _bookingService.GetBookingsByTourGuide(rentalDate, status, scheduleId, minTravelers, maxTravelers, sortBy, ascending);
            return Ok(new BaseResponseModel<List<GetBookingGuideModel>>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

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
    }
}
