using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.ReviewModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public ReviewService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<IEnumerable<ResponseReviewModel>> GetAllAsync(int? reviewType = null, double? minRating = null, double? maxRating = null, Guid? travelerId = null, Guid? localGuideId = null, bool? sortByRatingAscending = null, bool? sortByDateAscending = null)
        {
            var query = _unitOfWork.GetRepository<Review>().Entities.Where(r => !r.DeletedTime.HasValue);

            if (reviewType.HasValue)
                query = query.Where(r => r.ReviewType == (ReviewType)reviewType);

            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating);

            if (maxRating.HasValue)
                query = query.Where(r => r.Rating <= maxRating);

            if (travelerId.HasValue)
                query = query.Where(r => r.TravelerId == travelerId);

            if (localGuideId.HasValue)
                query = query.Where(r => r.LocalGuideId == localGuideId);

            if (sortByDateAscending.HasValue)
            {
                query = sortByDateAscending.Value
                    ? query.OrderBy(r => r.CreatedTime)
                    : query.OrderByDescending(r => r.CreatedTime);
            }

            if (sortByRatingAscending.HasValue)
            {
                query = sortByRatingAscending.Value
                    ? query.OrderBy(r => r.Rating)
                    : query.OrderByDescending(r => r.Rating);
            }

            var reviews = await query.ProjectTo<ResponseReviewModel>(_mapper.ConfigurationProvider).ToListAsync();

            return reviews;
        }

        public async Task CreateTourReviewAsync(string bookingId, CreateReviewModel model)
        {
            string strUserId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(strUserId, out Guid userId);

            var booking = await _unitOfWork.GetRepository<Booking>().FindAsync(b => b.Id == bookingId && (b.Status == BookingStatus.Completed || b.Status == BookingStatus.Refunded));

            if (booking == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Booking không tồn tại hoặc chưa hoàn thành.");
            }

            if (booking.UserId != userId)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Chỉ người dùng đã đặt booking mới có thể đánh giá.");
            }

            var existingReview = await _unitOfWork.GetRepository<Review>().FindAsync(r => r.BookingId == bookingId && r.TravelerId == userId && r.ReviewType == ReviewType.TourReview && !r.DeletedTime.HasValue );

            if (existingReview != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Bạn đã đánh giá cho booking này rồi.");
            }

            if (model.MediaIds == null || model.MediaIds.Count < 1 || model.MediaIds.Count > 5)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Phải có ít nhất 1 và tối đa 5 phương tiện.");
            }

            var newReview = new Review
            {
                Rating = model.Rating,
                Content = model.Content,
                TourId = booking.Schedule.TourId,
                TravelerId = userId,
                LocalGuideId = booking.Schedule.Tour.UserId,
                ReviewType = ReviewType.TourReview,
                CreatedBy = strUserId,
                LastUpdatedBy = strUserId,
                BookingId = bookingId
            };

            await _unitOfWork.GetRepository<Review>().InsertAsync(newReview);
            await _unitOfWork.SaveAsync();

            foreach (var mediaId in model.MediaIds)
            {
                var mediaExists = await _unitOfWork.GetRepository<Media>().FindAsync(m => m.Id == mediaId);

                if (mediaExists == null)
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Media với ID {mediaId} không tồn tại.");
                }

                mediaExists.ReviewId = newReview.Id;

                await _unitOfWork.GetRepository<Media>().UpdateAsync(mediaExists);
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task CreateLocalGuideReviewAsync(Guid localGuideId, CreateReviewModel model)
        {
            string strUserId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(strUserId, out Guid userId);

            var reviewExists = await _unitOfWork.GetRepository<Review>().Entities.AnyAsync(r => r.TravelerId == userId && r.LocalGuideId == localGuideId && r.ReviewType == ReviewType.LocalGuideReview && !r.DeletedTime.HasValue);

            if (reviewExists)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Bạn chỉ có thể đánh giá hướng dẫn viên một lần.");
            }

            var hasBookedWithLocalGuide = await _unitOfWork.GetRepository<Booking>().Entities.AnyAsync(b => b.UserId == userId && (b.Status == BookingStatus.Completed || b.Status == BookingStatus.Refunded) && b.Schedule.Tour.UserId == localGuideId && !b.DeletedTime.HasValue);

            if (!hasBookedWithLocalGuide)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Bạn phải có ít nhất một booking hoàn thành với hướng dẫn viên này để có thể đánh giá.");
            }

            if (model.MediaIds == null || model.MediaIds.Count < 1 || model.MediaIds.Count > 5)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Phải có ít nhất 1 và tối đa 5 phương tiện.");
            }

            var newReview = new Review
            {
                Rating = model.Rating,
                Content = model.Content,
                TravelerId = userId,
                LocalGuideId = localGuideId,
                ReviewType = ReviewType.LocalGuideReview,
                CreatedBy = strUserId,
                LastUpdatedBy = strUserId
            };

            await _unitOfWork.GetRepository<Review>().InsertAsync(newReview);
            await _unitOfWork.SaveAsync();

            foreach (var mediaId in model.MediaIds)
            {
                var mediaExists = await _unitOfWork.GetRepository<Media>().Entities
                    .FirstOrDefaultAsync(m => m.Id == mediaId);

                if (mediaExists == null)
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, $"Media với ID {mediaId} không tồn tại.");
                }

                mediaExists.ReviewId = newReview.Id;

                await _unitOfWork.GetRepository<Media>().UpdateAsync(mediaExists);
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(string id)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var review = await _unitOfWork.GetRepository<Review>().FindAsync(r => r.Id == id && !r.DeletedTime.HasValue);

            if (review == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Review không tồn tại.");
            }

            review.DeletedTime = CoreHelper.SystemTimeNow;
            review.DeletedBy = userId;

            await _unitOfWork.GetRepository<Review>().UpdateAsync(review);
            await _unitOfWork.SaveAsync();
        }

    }
}
