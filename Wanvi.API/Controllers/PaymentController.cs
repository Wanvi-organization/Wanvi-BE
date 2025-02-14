using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
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
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink(CreatePayOSPaymentRequest request)
        {
            string checkoutUrl = await _paymentService.CreatePayOSPaymentLink(request);
            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS, // Thay bằng hằng số của bạn
                data: checkoutUrl
            ));
        }   
    }
}
