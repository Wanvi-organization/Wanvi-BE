using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.VietMapModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        /// <summary>
        /// Tìm kiếm địa chỉ dựa trên từ khóa nhập vào (tích hợp với API VietMap)
        /// </summary>
        [HttpGet("Get_All_Addresses")]
        public async Task<IActionResult> SearchAddress([FromQuery] string query)
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseAutocompleteModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _addressService.SearchAsync(query)));
        }

        /// <summary>
        /// Lấy thông tin chi tiết địa chỉ dựa trên refid từ API VietMap và lưu vào database nếu chưa có.
        /// </summary>
        /// <param name="refId">ID của địa chỉ từ API VietMap</param>
        [HttpGet("Get_Address_By_RefId/{refId}")]
        public async Task<IActionResult> GetAddressDetails(string refId)
        {
            return Ok(new BaseResponseModel<ResponsePlaceModel>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _addressService.GetOrCreateAddressByRefIdAsync(refId)));
        }
    }
}
