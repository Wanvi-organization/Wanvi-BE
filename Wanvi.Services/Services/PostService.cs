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
            return _mapper.Map<IEnumerable<ResponsePostModel>>(posts).OrderByDescending(x => x.CreatedTime);
        }

        public async Task<IEnumerable<ResponsePostModel>> GetPostsByUserIdAsync(Guid userId)
        {
            var posts = await _unitOfWork.GetRepository<Post>().FindAllAsync(p => p.UserId == userId && !p.DeletedTime.HasValue);
            return _mapper.Map<IEnumerable<ResponsePostModel>>(posts).OrderByDescending(x => x.CreatedTime);
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

            if (model.Hashtags != null && model.Hashtags.Any())
            {
                foreach (var hashtagName in model.Hashtags)
                {
                    var existingHashtag = await _unitOfWork.GetRepository<Hashtag>()
                        .FindAsync(x => x.Name.ToLower() == hashtagName.ToLower());

                    Hashtag hashtagToUse = existingHashtag;
                    if (hashtagToUse == null)
                    {
                        hashtagToUse = new Hashtag { Name = hashtagName };
                        await _unitOfWork.GetRepository<Hashtag>().InsertAsync(hashtagToUse);
                        await _unitOfWork.SaveAsync();
                    }

                    await _unitOfWork.GetRepository<PostHashtag>().InsertAsync(new PostHashtag
                    {
                        PostId = newPost.Id,
                        HashtagId = hashtagToUse.Id
                    });
                }
            }

            if (model.MediaIds == null || !model.MediaIds.Any())
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Bài đăng phải có ít nhất 1 hình ảnh hoặc video.");
            }

            if (model.MediaIds.Count > 10)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Chỉ được phép đính kèm tối đa 10 hình ảnh hoặc video.");
            }

            foreach (var mediaId in model.MediaIds)
            {
                var media = await _unitOfWork.GetRepository<Media>().FindAsync(x => x.Id == mediaId);
                if (media != null)
                {
                    media.PostId = newPost.Id.ToString();
                    _unitOfWork.GetRepository<Media>().Update(media);
                }
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task UpdatePostAsync(string id, UpdatePostModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            var post = await _unitOfWork.GetRepository<Post>().GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bài đăng không tồn tại.");

            if (!string.IsNullOrWhiteSpace(model.Title))
                post.Title = model.Title;

            if (!string.IsNullOrWhiteSpace(model.Content))
                post.Content = model.Content;

            post.LastUpdatedTime = CoreHelper.SystemTimeNow;
            post.LastUpdatedBy = userId;

            var postHashtagRepo = _unitOfWork.GetRepository<PostHashtag>();
            if (model.Hashtags != null)
            {
                var oldHashtags = await postHashtagRepo.FindAllAsync(x => x.PostId == post.Id);
                foreach (var ph in oldHashtags)
                {
                    postHashtagRepo.Delete(ph);
                }

                foreach (var hashtagName in model.Hashtags)
                {
                    var hashtagRepo = _unitOfWork.GetRepository<Hashtag>();
                    var existingHashtag = await hashtagRepo.FindAsync(x => x.Name.ToLower() == hashtagName.ToLower());

                    Hashtag hashtagToUse = existingHashtag;
                    if (hashtagToUse == null)
                    {
                        hashtagToUse = new Hashtag { Name = hashtagName };
                        await hashtagRepo.InsertAsync(hashtagToUse);
                        await _unitOfWork.SaveAsync();
                    }

                    await postHashtagRepo.InsertAsync(new PostHashtag
                    {
                        PostId = post.Id,
                        HashtagId = hashtagToUse.Id
                    });
                }
            }

            var mediaRepo = _unitOfWork.GetRepository<Media>();
            if (model.MediaIds != null)
            {
                if (!model.MediaIds.Any())
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Bài đăng phải có ít nhất 1 hình ảnh hoặc video.");
                }

                if (model.MediaIds.Count > 10)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Chỉ được phép đính kèm tối đa 10 hình ảnh hoặc video.");
                }

                var oldMedias = await mediaRepo.FindAllAsync(x => x.PostId == post.Id.ToString());
                foreach (var media in oldMedias)
                {
                    media.PostId = null;
                    mediaRepo.Update(media);
                }

                foreach (var mediaId in model.MediaIds)
                {
                    var media = await mediaRepo.FindAsync(x => x.Id == mediaId);
                    if (media != null)
                    {
                        media.PostId = post.Id.ToString();
                        mediaRepo.Update(media);
                    }
                }
            }

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
