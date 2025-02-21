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
