using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.ModelViews.CityModelViews;
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

        [HttpGet("Get_Districts_By_City_Id")]
        public IActionResult GetDistrictsByCityId(string id)
        {
            var result = _districtService.GetByCityId(id);
            return Ok(BaseResponse<IEnumerable<ResponseDistrictModel>>.OkResponse(result));
        }
    }
}
