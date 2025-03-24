using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.RequestModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        /// <summary>
        /// Lấy danh sách yêu cầu theo trạng thái, loại yêu cầu và vai trò.
        /// </summary>
        /// <param name="roleId">Id của vai trò (Bắt buộc).</param>
        /// <param name="status">
        /// Trạng thái yêu cầu (tùy chọn):
        /// 0 - Đang chờ (Pending),
        /// 1 - Đã xác nhận (Confirmed),
        /// 2 - Đã hủy (Cancelled).
        /// </param>
        /// <param name="type">
        /// Loại yêu cầu (tùy chọn):
        /// 0 - Rút tiền từ booking (BookingWithdrawal),
        /// 1 - Rút tiền từ ví (BalanceWithdrawal),
        /// 2 - Khiếu nại (Complaint),
        /// 3 - Câu hỏi (Question).
        /// </param>
        [HttpGet("Get_All_Requests")]
        public async Task<IActionResult> GetAllRequests(Guid roleId, RequestStatus? status = null, RequestType? type = null)
        {
            var requests = await _requestService.GetAllAsync(roleId, status, type);
            return Ok(new BaseResponseModel<IEnumerable<ResponseRequestModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: requests));
        }
        /// <summary>
        /// Lấy yêu cầu bằng id.
        /// </summary>
        /// <param name="id">ID của yêu cầu cần lấy</param>
        [HttpGet("Get_Request_By_Id/{id}")]
        public async Task<IActionResult> GetRequestById(string id)
        {
            return Ok(new BaseResponseModel<ResponseRequestModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _requestService.GetByIdAsync(id)));
        }
        /// <summary>
        /// Admin xác nhận yêu cầu
        /// </summary>
        [HttpPatch("Confirm_From_Admin")]
        public async Task<IActionResult> ConfirmFromAdmin(AccecptRequestFromAdminModel model)
        {
            var requests = await _requestService.AccecptFromAdmin(model);
            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: requests));
        }
        /// <summary>
        /// Admin hủy yêu cầu
        /// </summary>
        [HttpPatch("Cancel_From_Admin")]
        public async Task<IActionResult> CancelFromAdmin(CancelRequestFromAdminModel model)
        {
            var requests = await _requestService.CancelFromAdmin(model);
            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: requests));
        }
        /// <summary>
        /// HDV/Khách hàng gửi yêu cầu: 0 là rút tiền(khi chọn 0 hiện đủ 3 trường để nhập), 1 là khiếu nại(hiện 2 trường:Type, Note), 2 là câu hỏi(hiện 2 trường Type, Note)
        /// </summary>
        [HttpPost("Create_Request")]
        public async Task<IActionResult> CreateRequest(CreateRequestModel model)
        {
            var requests = await _requestService.CreateRequest(model);
            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: requests));
        }
    }
}
