using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.CityModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;

        public CityController(ICityService cityService)
        {
            _cityService = cityService;
        }
        /// <summary>
        /// Lấy toàn bộ tỉnh/thành phố.
        /// </summary>
        [HttpGet("Get_All_Cities")]
        public async Task<IActionResult> GetAllCities()
        {
            return Ok(new BaseResponseModel<IEnumerable<ResponseCityModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: await _cityService.GetAllAsync()));
        }
    }
}
