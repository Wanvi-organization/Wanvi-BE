using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.EmailModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Gửi email từ Staff đến danh sách người dùng cụ thể hoặc tất cả người dùng theo RoleId.
        /// </summary>
        /// <param name="model">Thông tin email cần gửi, bao gồm danh sách userId hoặc RoleId.</param>
        [HttpPost("staff_send_email")]
        public async Task<IActionResult> StaffSendEmail([FromBody] SendEmailRequestModel model)
        {
            await _emailService.StaffSendEmailAsync(model);

            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: "Email đã được gửi thành công!"
            ));
        }
    }
}
