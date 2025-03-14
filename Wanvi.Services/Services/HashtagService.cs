using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.HashtagModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class HashtagService : IHashtagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public HashtagService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponseHashtagModel>> GetAllAsync()
        {
            var hashtags = await _unitOfWork.GetRepository<Hashtag>().FindAllAsync(h => !h.DeletedTime.HasValue);

            if (!hashtags.Any())
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NotFound, "Không có hashtag nào.");
            }

            return _mapper.Map<IEnumerable<ResponseHashtagModel>>(hashtags);
        }

        public async Task<ResponseHashtagModel> GetByIdAsync(string id)
        {
            var hashtag = await _unitOfWork.GetRepository<Hashtag>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hashtag không tồn tại.");

            if (hashtag.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hashtag đã bị xóa.");
            }

            return _mapper.Map<ResponseHashtagModel>(hashtag);
        }

        public async Task CreateAsync(CreateHashtagModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vui lòng điền tên hashtag.");
            }

            var newHashtag = _mapper.Map<Hashtag>(model);
            newHashtag.CreatedBy = userId;
            newHashtag.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Hashtag>().InsertAsync(newHashtag);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(string id, UpdateHashtagModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            var hashtag = await _unitOfWork.GetRepository<Hashtag>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hashtag không tồn tại.");

            if (hashtag.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hashtag đã bị xóa.");
            }

            _mapper.Map(model, hashtag);
            hashtag.LastUpdatedTime = CoreHelper.SystemTimeNow;
            hashtag.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Hashtag>().UpdateAsync(hashtag);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(string id)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var hashtag = await _unitOfWork.GetRepository<Hashtag>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hashtag không tồn tại.");

            if (hashtag.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Hashtag đã bị xóa.");
            }

            var isExistAnyPosts = await _unitOfWork.GetRepository<Post>().Entities.AnyAsync(p => p.PostHashtags.Any(t => t.HashtagId == id) && !p.DeletedTime.HasValue);

            if (isExistAnyPosts)
            {
                throw new ErrorException(StatusCodes.Status409Conflict, ResponseCodeConstants.FAILED, "Không thể xóa vì vẫn còn bài viết chứa hashtag này.");
            }

            hashtag.LastUpdatedTime = CoreHelper.SystemTimeNow;
            hashtag.LastUpdatedBy = userId;
            hashtag.DeletedTime = CoreHelper.SystemTimeNow;
            hashtag.DeletedBy = userId;

            await _unitOfWork.GetRepository<Hashtag>().UpdateAsync(hashtag);
            await _unitOfWork.SaveAsync();
        }
    }
}
