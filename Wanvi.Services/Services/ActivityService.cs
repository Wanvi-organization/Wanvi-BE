using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.ActivityModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public ActivityService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponseActivityModel>> GetAllAsync()
        {
            var activities = await _unitOfWork.GetRepository<Activity>().FindAllAsync(a => !a.DeletedTime.HasValue);

            if (!activities.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Hoạt động không tồn tại.");
            }

            return _mapper.Map<IEnumerable<ResponseActivityModel>>(activities);
        }

        public async Task<ResponseActivityModel> GetByIdAsync(string id)
        {
            var activity = await _unitOfWork.GetRepository<Activity>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động không tồn tại.");

            if (activity.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động đã bị xóa.");
            }

            return _mapper.Map<ResponseActivityModel>(activity);
        }

        public async Task CreateAsync(CreateActivityModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vui lòng điền tên hoạt động.");
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vui lòng điền mô tả hoạt động.");
            }

            var newActivity = _mapper.Map<Activity>(model);

            newActivity.CreatedBy = userId;
            newActivity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Activity>().InsertAsync(newActivity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(string id, UpdateActivityModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            var activity = await _unitOfWork.GetRepository<Activity>().GetByIdAsync(id.Trim()) 
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động không tồn tại.");

            if (activity.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động đã bị xóa.");
            }

            if (model.Name != null && string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Tên hoạt động không hợp lệ.");
            }

            if (model.Description != null && string.IsNullOrWhiteSpace(model.Description))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.INVALID_INPUT, "Mô tả hoạt động không hợp lệ.");
            }

            _mapper.Map(model, activity);
            activity.LastUpdatedTime = CoreHelper.SystemTimeNow;
            activity.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Activity>().UpdateAsync(activity);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(string id)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var activity = await _unitOfWork.GetRepository<Activity>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động không tồn tại.");

            if (activity.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hoạt động đã bị xóa.");
            }

            var isExistAnyTours = await _unitOfWork.GetRepository<Tour>().Entities.AnyAsync(p => p.TourActivities.Any(t => t.ActivityId == id) && !p.DeletedTime.HasValue);

            if (isExistAnyTours)
            {
                throw new ErrorException(StatusCodes.Status409Conflict, ResponseCodeConstants.FAILED, "Không thể xóa vì vẫn còn tour chứa hoạt động này.");
            }

            activity.LastUpdatedTime = CoreHelper.SystemTimeNow;
            activity.LastUpdatedBy = userId;
            activity.DeletedTime = CoreHelper.SystemTimeNow;
            activity.DeletedBy = userId;

            await _unitOfWork.GetRepository<Activity>().UpdateAsync(activity);
            await _unitOfWork.SaveAsync();
        }
    }
}

