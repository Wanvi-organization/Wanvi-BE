using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
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

        [HttpGet("Get_All_Cities")]
        public IActionResult GetAllCities()
        {
            var result = _cityService.GetAll();
            return Ok(BaseResponse<IEnumerable<ResponseCityModel>>.OkResponse(result));
        }
    }
}
