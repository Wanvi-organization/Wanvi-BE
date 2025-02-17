using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.DistrictModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistrictController : ControllerBase
    {
        private readonly IDistrictService _districtService;

        public DistrictController(IDistrictService districtService)
        {
            _districtService = districtService;
        }
        /// <summary>
        /// Lấy quận/huyện bằng id tinh/thành phố.
        /// </summary>
        /// <param name="id">ID của tỉnh/thành phố cần lấy quận/huyện</param>
        [HttpGet("Get_Districts_By_City_Id")]
        public async Task<IActionResult> GetDistrictsByCityId(string cityId)
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseDistrictModel>>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: await _districtService.GetAllByCityId(cityId)));

        }
    }
}
