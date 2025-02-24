using AutoMapper;
using Microsoft.AspNetCore.Http;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.NewsModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class NewsService : INewsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public NewsService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponseNewsModel>> GetAllAsync()
        {
            var news = await _unitOfWork.GetRepository<News>().FindAllAsync(a => !a.DeletedTime.HasValue);

            if (!news.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Tin tức không tồn tại.");
            }

            return _mapper.Map<IEnumerable<ResponseNewsModel>>(news);
        }

        public async Task<ResponseNewsModel> GetByIdAsync(string id)
        {
            var activity = await _unitOfWork.GetRepository<Activity>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động không tồn tại.");

            if (activity.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động đã bị xóa.");
            }

            return _mapper.Map<ResponseNewsModel>(activity);
        }

        public async Task CreateAsync(CreateNewsModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            var sortOrders = model.NewsDetails.Select(d => d.SortOrder).ToList();
            if (sortOrders.Distinct().Count() != sortOrders.Count)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Thứ tự sắp xếp không được trùng lặp.");
            }

            var newNews = new News
            {
                Title = model.Title,
                Summary = model.Summary,
                CategoryId = model.CategoryId,
                UserId = Guid.Parse(userId),
                NewsDetails = new List<NewsDetail>()
            };

            newNews.CreatedBy = userId.ToString();
            newNews.LastUpdatedBy = userId.ToString();

            foreach (var newsDetail in model.NewsDetails)
            {
                newNews.NewsDetails.Add(new NewsDetail
                {
                    NewsId = newNews.Id.ToString(),
                    Url = newsDetail.Url,
                    Content = newsDetail.Content,
                    SortOrder = newsDetail.SortOrder
                });
            }

            await _unitOfWork.GetRepository<News>().InsertAsync(newNews);
            await _unitOfWork.SaveAsync();
        }

        //public async Task UpdateAsync(string id, UpdateActivityModel model)
        //{
        //    string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
        //    model.TrimAllStrings();

        //    var activity = await _unitOfWork.GetRepository<Activity>().GetByIdAsync(id.Trim())
        //        ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động không tồn tại.");

        //    if (activity.DeletedTime.HasValue)
        //    {
        //        throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động đã bị xóa.");
        //    }

        //    if (model.Name != null && string.IsNullOrWhiteSpace(model.Name))
        //    {
        //        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Tên hoạt động không hợp lệ.");
        //    }

        //    if (model.Description != null && string.IsNullOrWhiteSpace(model.Description))
        //    {
        //        throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Mô tả hoạt động không hợp lệ.");
        //    }

        //    _mapper.Map(model, activity);
        //    activity.LastUpdatedTime = CoreHelper.SystemTimeNow;
        //    activity.LastUpdatedBy = userId;

        //    await _unitOfWork.GetRepository<Activity>().UpdateAsync(activity);
        //    await _unitOfWork.SaveAsync();
        //}

        //public async Task DeleteAsync(string id)
        //{
        //    string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

        //    var activity = await _unitOfWork.GetRepository<Activity>().GetByIdAsync(id.Trim())
        //        ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động không tồn tại.");

        //    if (activity.DeletedTime.HasValue)
        //    {
        //        throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động đã bị xóa.");
        //    }

        //    var isExistAnyTours = _unitOfWork.GetRepository<Tour>().Entities.Any(p => p.TourActivities.Any(t => t.ActivityId == id) && !p.DeletedTime.HasValue);

        //    if (isExistAnyTours)
        //    {
        //        throw new ErrorException(StatusCodes.Status409Conflict, ResponseCodeConstants.FAILED, "Không thể xóa vì vẫn còn tour chứa hoạt động này.");
        //    }

        //    activity.LastUpdatedTime = CoreHelper.SystemTimeNow;
        //    activity.LastUpdatedBy = userId;
        //    activity.DeletedTime = CoreHelper.SystemTimeNow;
        //    activity.DeletedBy = userId;

        //    await _unitOfWork.GetRepository<Activity>().UpdateAsync(activity);
        //    await _unitOfWork.SaveAsync();
        //}
    }
}
