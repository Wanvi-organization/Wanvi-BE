using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.PaymentModelViews;

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

        //[HttpPost("create_payment_link")]
        //public async Task<IActionResult> CreatePaymentLink(CreatePayOSPaymentRequest request)
        //{
        //    string checkoutUrl = await _paymentService.CreatePayOSPaymentLink(request);
        //    return Ok(new BaseResponseModel<string>(
        //        statusCode: StatusCodes.Status200OK,
        //        code: ResponseCodeConstants.SUCCESS, // Thay bằng hằng số của bạn
        //        data: checkoutUrl
        //    ));
        //}   
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
