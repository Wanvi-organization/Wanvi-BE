using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.ModelViews.TourModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;

        public TourController(ITourService tourService)
        {
            _tourService = tourService;
        }

        [HttpPost("Create_Tour")]
        public async Task<IActionResult> CreateTour(CreateTourModel model)
        {
            var result = await _tourService.CreateTourAsync(model);
            return Ok(BaseResponse<Tour>.OkResponse(result));
        }
    }
}
