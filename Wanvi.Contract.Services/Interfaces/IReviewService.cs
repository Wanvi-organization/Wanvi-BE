using Wanvi.ModelViews.ReviewModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ResponseReviewModel>> GetAllAsync(int? reviewType = null, double? minRating = null, double? maxRating = null, Guid? travelerId = null, Guid? localGuideId = null, bool? sortByRatingAscending = null, bool? sortByDateAscending = null);
        Task CreateTourReviewAsync(string bookingId, CreateReviewModel model);
        Task CreateLocalGuideReviewAsync(Guid localGuideId, CreateReviewModel model);
        Task DeleteAsync(string id);
    }
}
