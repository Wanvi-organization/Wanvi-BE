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
        /// Lấy danh sách yêu cầu với tùy chọn lọc theo trạng thái.
        /// </summary>
        /// <param name="status">
        /// Trạng thái yêu cầu (tùy chọn):
        /// 0 - Pending (Đang chờ xử lý),
        /// 1 - Confirmed (Đã xác nhận),
        /// 2 - Cancelled (Đã hủy).
        /// </param>
        [HttpGet("Get_All_Requests")]
        public async Task<IActionResult> GetAllRequests(RequestStatus? status = null)
        {
            var requests = await _requestService.GetAllAsync(status);
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
    }
}
