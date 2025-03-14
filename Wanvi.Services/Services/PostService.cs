using AutoMapper;
using Microsoft.AspNetCore.Http;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.PostModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public PostService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponsePostModel>> GetAllPostsAsync()
        {
            var posts = await _unitOfWork.GetRepository<Post>().FindAllAsync(p => !p.DeletedTime.HasValue);
            return _mapper.Map<IEnumerable<ResponsePostModel>>(posts);
        }

        public async Task<ResponsePostModel> GetPostByIdAsync(string id)
        {
            var post = await _unitOfWork.GetRepository<Post>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bài đăng không tồn tại.");

            return _mapper.Map<ResponsePostModel>(post);
        }

        public async Task CreatePostAsync(CreatePostModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Content))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tiêu đề và nội dung không được để trống.");
            }

            var newPost = _mapper.Map<Post>(model);
            newPost.UserId = Guid.Parse(userId);
            newPost.LikeCount = 0;

            await _unitOfWork.GetRepository<Post>().InsertAsync(newPost);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdatePostAsync(string id, UpdatePostModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            var post = await _unitOfWork.GetRepository<Post>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bài đăng không tồn tại.");

            _mapper.Map(model, post);
            post.LastUpdatedTime = CoreHelper.SystemTimeNow;
            post.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Post>().UpdateAsync(post);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeletePostAsync(string id)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var post = await _unitOfWork.GetRepository<Post>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bài đăng không tồn tại.");

            post.DeletedTime = CoreHelper.SystemTimeNow;
            post.DeletedBy = userId;

            await _unitOfWork.GetRepository<Post>().UpdateAsync(post);
            await _unitOfWork.SaveAsync();
        }

        public async Task LikePostAsync(string id)
        {
            var post = await _unitOfWork.GetRepository<Post>().GetByIdAsync(id.Trim());
            if (post == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bài đăng không tồn tại.");
            }
            post.LikeCount++;
            await _unitOfWork.SaveAsync();
        }

        public async Task UnlikePostAsync(string id)
        {
            var post = await _unitOfWork.GetRepository<Post>().GetByIdAsync(id.Trim());
            if (post == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bài đăng không tồn tại.");
            }
            post.LikeCount--;
            await _unitOfWork.SaveAsync();
        }
    }
}
