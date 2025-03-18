using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Repositories.Base;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.ReviewModelViews;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Truy vấn danh sách các review theo nhiều tiêu chí lọc và sắp xếp.
        /// </summary>
        /// <param name="reviewType">Loại review (0 - TourReview, 1 - LocalGuideReview, tùy chọn).</param>
        /// <param name="minRating">Đánh giá tối thiểu (tùy chọn).</param>
        /// <param name="maxRating">Đánh giá tối đa (tùy chọn).</param>
        /// <param name="travelerId">ID của traveler (tùy chọn).</param>
        /// <param name="localGuideId">ID của local guide (tùy chọn).</param>
        /// <param name="sortByRatingAscending">Sắp xếp theo rating (true - tăng dần, false - giảm dần, null - không sắp xếp).</param>
        /// <param name="sortByDateAscending">Sắp xếp theo ngày tạo (true - tăng dần, false - giảm dần, null - không sắp xếp).</param>
        [HttpGet("Get_Reviews")]
        public async Task<IActionResult> GetReviews(
            [FromQuery] int? reviewType = null,
            [FromQuery] double? minRating = null,
            [FromQuery] double? maxRating = null,
            [FromQuery] Guid? travelerId = null,
            [FromQuery] Guid? localGuideId = null,
            [FromQuery] bool? sortByRatingAscending = null,
            [FromQuery] bool? sortByDateAscending = null)
        {
            var reviews = await _reviewService.GetAllAsync(
                reviewType,
                minRating,
                maxRating,
                travelerId,
                localGuideId,
                sortByRatingAscending,
                sortByDateAscending);

            return Ok(new BaseResponseModel<IEnumerable<ResponseReviewModel>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: reviews));
        }

        /// <summary>
        /// Tạo review cho Tour sau khi hoàn thành booking.
        /// </summary>
        /// <param name="bookingId">ID của booking.</param>
        /// <param name="model">Thông tin review cần tạo.</param>
        [HttpPost("Create_Tour_Review/{bookingId}")]
        public async Task<IActionResult> CreateTourReviewAsync(string bookingId, [FromBody] CreateReviewModel model)
        {
            await _reviewService.CreateTourReviewAsync(bookingId, model);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Review cho tour đã được tạo thành công."));
        }

        /// <summary>
        /// Tạo review cho Local Guide.
        /// </summary>
        /// <param name="localGuideId">ID của hướng dẫn viên.</param>
        /// <param name="model">Thông tin review cần tạo.</param>
        [HttpPost("Create_Local_Guide_Review/{localGuideId}")]
        public async Task<IActionResult> CreateLocalGuideReviewAsync(Guid localGuideId, [FromBody] CreateReviewModel model)
        {
            await _reviewService.CreateLocalGuideReviewAsync(localGuideId, model);
            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Review cho hướng dẫn viên đã được tạo thành công."));
        }

        /// <summary>
        /// Xóa review theo ID.
        /// </summary>
        /// <param name="id">ID của review cần xóa.</param>
        [HttpDelete("Delete_Review/{id}")]
        public async Task<IActionResult> DeleteReviewAsync(string id)
        {
            // Gọi service để xóa review
            await _reviewService.DeleteAsync(id);

            return Ok(new BaseResponseModel<string?>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                message: "Xóa review thành công."));
        }
    }
}
