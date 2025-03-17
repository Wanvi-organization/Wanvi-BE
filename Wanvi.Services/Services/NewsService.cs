using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
            var newsList = await _unitOfWork.GetRepository<News>().FindAllAsync(a => !a.DeletedTime.HasValue);

            if (!newsList.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Tin tức không tồn tại.");
            }

            foreach (var news in newsList)
            {
                news.NewsDetails = news.NewsDetails.OrderBy(nd => nd.SortOrder).ToList();
            }

            return _mapper.Map<IEnumerable<ResponseNewsModel>>(newsList);
        }

        public async Task<IEnumerable<ResponseNewsModel>> GetAllByCategoryIdAsync(string id)
        {
            var newsList = await _unitOfWork.GetRepository<News>().FindAllAsync(n => n.CategoryId == id.Trim() && !n.DeletedTime.HasValue);

            if (!newsList.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không có tin tức nào thuộc danh mục này.");
            }

            foreach (var news in newsList)
            {
                news.NewsDetails = news.NewsDetails.OrderBy(nd => nd.SortOrder).ToList();
            }

            return _mapper.Map<IEnumerable<ResponseNewsModel>>(newsList);
        }

        public async Task<ResponseNewsModel> GetByIdAsync(string id)
        {
            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tin tức không tồn tại.");

            if (news.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tin tức đã bị xóa.");
            }

            news.NewsDetails = news.NewsDetails.OrderBy(nd => nd.SortOrder).ToList();

            return _mapper.Map<ResponseNewsModel>(news);
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

            var categoryExists = await _unitOfWork.GetRepository<Category>().Entities.AnyAsync(c => c.Id == model.CategoryId);
            if (!categoryExists)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Danh mục không tồn tại.");
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

        public async Task UpdateAsync(string id, UpdateNewsModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tin tức không tồn tại.");

            if (news.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tin tức đã bị xóa.");
            }

            if (!string.IsNullOrEmpty(model.Title) && string.IsNullOrWhiteSpace(model.Title))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tiêu đề tin tức không hợp lệ.");
            }

            if (!string.IsNullOrEmpty(model.Summary) && string.IsNullOrWhiteSpace(model.Summary))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tóm tắt tin tức không hợp lệ.");
            }

            if (!string.IsNullOrEmpty(model.CategoryId))
            {
                bool categoryExists = await _unitOfWork.GetRepository<Category>()
                    .Entities.AnyAsync(c => c.Id == model.CategoryId.Trim());

                if (!categoryExists)
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Danh mục không tồn tại.");
                }
            }

            if (model.NewsDetails != null && model.NewsDetails.Count > 0)
            {
                var existingDetails = news.NewsDetails.ToList();
                var existingDetailIds = existingDetails.Select(nd => nd.Id).ToHashSet(); // Lưu danh sách ID cũ
                var newSortOrders = model.NewsDetails
                    .Where(d => d.SortOrder.HasValue)
                    .Select(d => d.SortOrder.Value)
                    .ToList();

                var existingSortOrders = existingDetails
                    .Select(nd => nd.SortOrder)
                    .ToList();

                // Kiểm tra trùng lặp `SortOrder` trong request
                if (newSortOrders.Distinct().Count() != newSortOrders.Count)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Thứ tự sắp xếp không được trùng lặp trong dữ liệu gửi lên.");
                }

                foreach (var newsDetailModel in model.NewsDetails)
                {
                    NewsDetail? existingDetail = null;

                    // Nếu có ID, kiểm tra xem có tồn tại không
                    if (!string.IsNullOrEmpty(newsDetailModel.Id))
                    {
                        existingDetail = existingDetails.FirstOrDefault(nd => nd.Id == newsDetailModel.Id);
                    }

                    if (existingDetail != null)
                    {
                        // Nếu `SortOrder` không được truyền lên, giữ nguyên giá trị cũ
                        if (!newsDetailModel.SortOrder.HasValue)
                        {
                            newsDetailModel.SortOrder = existingDetail.SortOrder;
                        }

                        // Nếu `SortOrder` thay đổi, kiểm tra trùng lặp
                        if (existingDetail.SortOrder != newsDetailModel.SortOrder)
                        {
                            if (existingDetails.Any(nd => nd.SortOrder == newsDetailModel.SortOrder && nd.Id != existingDetail.Id))
                            {
                                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Thứ tự sắp xếp không được trùng lặp.");
                            }
                            existingDetail.SortOrder = newsDetailModel.SortOrder.Value;
                        }

                        // Cập nhật nội dung
                        existingDetail.Url = newsDetailModel.Url;
                        existingDetail.Content = newsDetailModel.Content;

                        // Đánh dấu ID này đã được xử lý
                        existingDetailIds.Remove(existingDetail.Id);
                    }
                    else
                    {
                        // Nếu tạo mới mà không có `SortOrder`, báo lỗi
                        if (!newsDetailModel.SortOrder.HasValue)
                        {
                            throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "SortOrder không được để trống khi thêm mới.");
                        }

                        // Kiểm tra tránh thêm `SortOrder` trùng lặp
                        if (existingDetails.Any(nd => nd.SortOrder == newsDetailModel.SortOrder))
                        {
                            throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Thứ tự sắp xếp không được trùng lặp.");
                        }

                        var newDetail = new NewsDetail
                        {
                            NewsId = news.Id.ToString(),
                            Url = newsDetailModel.Url,
                            Content = newsDetailModel.Content,
                            SortOrder = newsDetailModel.SortOrder.Value
                        };

                        _unitOfWork.GetRepository<NewsDetail>().Insert(newDetail);
                        news.NewsDetails.Add(newDetail);
                    }
                }
            }

            _mapper.Map(model, news);

            news.LastUpdatedTime = CoreHelper.SystemTimeNow;
            news.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<News>().UpdateAsync(news);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(string id)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tin tức không tồn tại.");

            if (news.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tin tức đã bị xóa.");
            }

            news.LastUpdatedTime = CoreHelper.SystemTimeNow;
            news.LastUpdatedBy = userId;
            news.DeletedTime = CoreHelper.SystemTimeNow;
            news.DeletedBy = userId;

            await _unitOfWork.GetRepository<News>().UpdateAsync(news);
            await _unitOfWork.SaveAsync();
        }
    }
}
