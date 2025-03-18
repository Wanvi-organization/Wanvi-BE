using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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
        /// Tạo link thanh toán dành cho hóa đơn 100%
        /// </summary>
        [HttpPost("create_payment_all_link")]
        public async Task<IActionResult> CreatePaymentAllLink(CreatePayOSPaymentRequest request)
        {
            string checkoutUrl = await _paymentService.CreatePayOSPaymentAllLink(request);
            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS, // Thay bằng hằng số của bạn
                data: checkoutUrl
            ));
        }
        /// <summary>
        /// Tạo link thanh toán dành cho hóa đơn cọc 50% đầu
        /// </summary>
        [HttpPost("create_payment_haft_link")]
        public async Task<IActionResult> CreatePaymentHaftLink(CreatePayOSPaymentRequest request)
        {
            string checkoutUrl = await _paymentService.CreatePayOSPaymentHaftLink(request);
            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS, // Thay bằng hằng số của bạn
                data: checkoutUrl
            ));
        }

        /// <summary>
        /// Nạp tiền vào tài khoản 
        /// </summary>
        [HttpPost("Deposit_Money")]
        public async Task<IActionResult> DepositMoney(DepositMoneyRequest request)
        {
            string res = await _paymentService.DepositMoney(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        /// <summary>
        /// Tạo link thanh toán dành cho hóa đơn trả 50% sau
        /// </summary>
        [HttpPost("create_payment_haft_end_link")]
        public async Task<IActionResult> CreateBookingHaftEndLink(CreateBookingEndModel request)
        {
            string res = await _paymentService.CreateBookingHaftEnd(request);
            return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: res
             ));
        }
        [AllowAnonymous]
        [HttpPost("payos_callback")]
        public async Task<IActionResult> PayOSCallback([FromBody] PayOSWebhookRequest request)
        {
            try
            {
                string jsonRequest = JsonSerializer.Serialize(request);
                Console.WriteLine($"📌 Received Webhook Data: {jsonRequest}");
                //Console.WriteLine($"📌 Signature: {signature}");

                // Nếu request null, trả về lỗi
                if (request == null || request.data == null)
                {
                    return BadRequest(new { message = "Dữ liệu webhook không hợp lệ" });
                }

                // 🚀 Nếu request từ PayOS kiểm tra Webhook, bỏ qua xử lý nhưng vẫn trả về 200 OK
                if (request.data.orderCode == null)
                {
                    Console.WriteLine("📌 PayOS Webhook Verification - Skipping Processing");
                    return Ok(new { message = "Webhook verified successfully" });
                }

                // Xử lý khi có orderCode thật từ PayOS
                await _paymentService.PayOSCallback(request);
                return Ok(new { message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Webhook Error: {ex.Message}");
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }



    }
}
