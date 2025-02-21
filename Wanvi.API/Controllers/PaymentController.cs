using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.BookingModelViews;
using Wanvi.ModelViews.PaymentModelViews;
using Wanvi.Services.Services;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _unitOfWork;
        public PaymentController(IPaymentService paymentService, IUnitOfWork unitOfWork)
        {
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// Tạo link thanh toán dành cho hóa đơn mới bắt đầu tạo(cả 100% và 50%)
        /// </summary>
        [HttpPost("create_payment_link")]
        public async Task<IActionResult> CreatePaymentLink(CreatePayOSPaymentRequest request)
        {
            string checkoutUrl = await _paymentService.CreatePayOSPaymentLink(request);
            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS, // Thay bằng hằng số của bạn
                data: checkoutUrl
            ));
        }
        /// <summary>
        /// Tạo link thanh toán dành cho hóa đơn lần 2(dành cho đã cọc 50%)
        /// </summary>
        [HttpPost("create_payment_link_end")]
        public async Task<IActionResult> CreateBookingHaftEnd(CreateBookingEndModel request)
        {
            string res = await _paymentService.CreateBookingHaftEnd(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        // Trong PayOSController.cs
        [HttpPost("payos_callback")]
        public async Task<IActionResult> PayOSCallback([FromBody] PayOSWebhookRequest request, [FromHeader(Name = "x-payos-signature")] string signature)
        {
            await _paymentService.PayOSCallback(request, signature);

            // 4. Trả về response cho PayOS (thường là 200 OK)
            return Ok();
        }
    }
}
