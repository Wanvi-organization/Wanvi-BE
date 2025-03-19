using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.BookingModelViews;
using Wanvi.ModelViews.DashboardModelViews;
using Wanvi.ModelViews.PaymentModelViews;
using Wanvi.ModelViews.TourModelViews;
using Wanvi.Services.Services;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly ITourService _tourService;

        public DashboardController(IDashboardService dashboardService, IPaymentService paymentService, IBookingService bookingService, ITourService tourService)
        {
            _dashboardService = dashboardService;
            _paymentService = paymentService;
            _bookingService = bookingService;
            _tourService = tourService;
        }

        [HttpGet("Get_All_Dashboard_Data")]
        public async Task<IActionResult> GetAllDashboardData()
        {
            var result = await _dashboardService.GetDashboardDataAsync();
            return Ok(BaseResponse<ResponseDashboardModel>.OkResponse(result));
        }
        /// <summary>
        /// Tổng hợp giao dịch trên app, khi ko chọn gì mặc định lấy năm hiện tại (thành công, thất bại, nạp tiền,...), thứ tự ưu tiên day>month>year
        /// </summary>
        /// <param name="day">format điền vào là:21/01/2024</param>
        /// <param name="month">format điền vào là:01/2024</param>
        /// <param name="year">format điền vào là:2024</param>
        /// <param name="status">0 là Chưa thanh toán, 1 là Đã thanh toán, 2 là Đã hoàn tiền, 3 là Đã hủy, 4 là Chưa nạp tiền, 5 là Đã nạp tiền</param>
        [HttpGet("Transaction_Summary")]
        public async Task<IActionResult> TransactionSummary(string? day, string? month, int? year, PaymentStatus? status)
        {
            var res = await _paymentService.TransactionSummary(day, month, year, status);
            return Ok(new BaseResponseModel<TransactionSummaryModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS, // Thay bằng hằng số của bạn
                data: res
            ));
        }
        /// <summary>
        /// Thống kê booking, khi ko chọn gì mặc định lấy năm hiện tại, thứ tự ưu tiên day>month>year
        /// </summary>
        /// <param name="day">format điền vào là:21/01/2024</param>
        /// <param name="month">format điền vào là:01/2024</param>
        /// <param name="year">format điền vào là:2024</param>
        [HttpGet("Booking_Statistics")]
        public async Task<IActionResult> BookingStatistics(string? day, string? month, int? year)
        {
            var res = await _bookingService.BookingStatistics(day, month, year);
            return Ok(new BaseResponseModel<List<BookingStatisticsModel>>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Tổng hợp doanh thu, khi ko chọn gì mặc định lấy năm hiện tại, thứ tự ưu tiên day>month>year
        /// </summary>
        /// <param name="day">format điền vào là:21/01/2024</param>
        /// <param name="month">format điền vào là:01/2024</param>
        /// <param name="year">format điền vào là:2024</param>
        [HttpGet("Total_Revenue")]
        public async Task<IActionResult> TotalRevenue(string? day, string? month, int? year)
        {
            var res = await _bookingService.TotalRevenue(day, month, year);
            return Ok(new BaseResponseModel<List<TotalRevenueModel>>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }

        /// <summary>
        /// Thống kê tour theo ngày, tháng, năm. Các tham số có thể được cung cấp để lọc theo thời gian cụ thể.
        /// </summary>
        /// <param name="day">Ngày cần thống kê (định dạng: dd/MM/yyyy).</param>
        /// <param name="month">Tháng cần thống kê (định dạng: MM/yyyy).</param>
        /// <param name="year">Năm cần thống kê (định dạng: yyyy).</param>
        [HttpGet("Tour_Statistics")]
        public async Task<IActionResult> GetTourStatistics(string? day, string? month, int? year)
        {
            var res = await _tourService.GetTourStatistics(day, month, year);
            return Ok(new BaseResponseModel<TourStatisticsModel>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
    }
}
