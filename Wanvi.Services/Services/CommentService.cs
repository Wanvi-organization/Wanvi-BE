using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.CommentModelViews;
using Wanvi.Services.Services.Infrastructure;

namespace Wanvi.Services.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public CommentService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponseCommentModel>> GetAllCommentsByNewsIdAsync(string newsId)
        {
            var newsExists = await _unitOfWork.GetRepository<News>().Entities.AnyAsync(n => n.Id == newsId);

            if (!newsExists)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tin tức không tồn tại.");
            }

            var comments = await _unitOfWork.GetRepository<Comment>().Entities
            .Where(c => c.NewsId == newsId && !c.DeletedTime.HasValue)
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ToListAsync();

            var commentResponseList = _mapper.Map<IEnumerable<ResponseCommentModel>>(comments);

            return commentResponseList;
        }

        public async Task CreateNewsCommentAsync(string newsId, CreateCommentModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Vui lòng điền nội dung bình luận.");
            }

            var news = await _unitOfWork.GetRepository<News>().GetByIdAsync(newsId) ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Tin tức không tồn tại.");

            var newComment = _mapper.Map<Comment>(model);
            newComment.NewsId = newsId;
            newComment.UserId = Guid.Parse(userId);
            newComment.CreatedBy = userId;
            newComment.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Comment>().InsertAsync(newComment);
            await _unitOfWork.SaveAsync();
        }

        public async Task ReplyCommentAsync(string commentId, CreateCommentModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Vui lòng điền nội dung trả lời.");
            }

            var parentComment = await _unitOfWork.GetRepository<Comment>().GetByIdAsync(commentId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bình luận cần trả lời không tồn tại.");

            var replyComment = _mapper.Map<Comment>(model);
            replyComment.ParentCommentId = commentId;
            replyComment.UserId = Guid.Parse(userId);
            replyComment.CreatedBy = userId;
            replyComment.LastUpdatedBy = userId;

            await _unitOfWork.GetRepository<Comment>().InsertAsync(replyComment);
            await _unitOfWork.SaveAsync();
        }

        public async Task<bool> LikeUnlikeCommentAsync(string commentId)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var comment = await _unitOfWork.GetRepository<Comment>().GetByIdAsync(commentId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bình luận không tồn tại.");

            if (!comment.IsLikedByUser)
            {
                comment.IsLikedByUser = true;
                comment.LikeCount++;
                await _unitOfWork.GetRepository<Comment>().UpdateAsync(comment);
                await _unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                comment.IsLikedByUser = false;
                comment.LikeCount--;
                await _unitOfWork.GetRepository<Comment>().UpdateAsync(comment);
                await _unitOfWork.SaveAsync();
                return false;
            }
        }

        public async Task UpdateCommentAsync(string commentId, UpdateCommentModel model)
        {
            string strUserId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(strUserId, out Guid userId);

            var user = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(userId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Người dùng không tồn tại.");

            var userRoles = await _unitOfWork.GetRepository<ApplicationUserRole>().Entities.Where(ur => ur.UserId == userId).ToListAsync();

            var roleNames = userRoles.Select(ur => ur.Role.Name).ToList();

            var comment = await _unitOfWork.GetRepository<Comment>().GetByIdAsync(commentId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bình luận không tồn tại.");

            if (comment.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bình luận đã bị xóa.");
            }

            if (userId != comment.UserId && !roleNames.Contains("Admin") && !roleNames.Contains("Staff"))
            {
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN, "Bạn không có quyền sửa bình luận này.");
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Nội dung bình luận không hợp lệ.");
            }

            _mapper.Map(model, comment);
            comment.LastUpdatedBy = userId.ToString();
            comment.LastUpdatedTime = CoreHelper.SystemTimeNow;

            await _unitOfWork.GetRepository<Comment>().UpdateAsync(comment);
            await _unitOfWork.SaveAsync();
        }


        public async Task DeleteCommentAsync(string commentId)
        {
            string strUserId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            Guid.TryParse(strUserId, out Guid userId);

            var user = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(userId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Người dùng không tồn tại.");

            var userRoles = await _unitOfWork.GetRepository<ApplicationUserRole>().Entities.Where(ur => ur.UserId == userId).ToListAsync();

            var roleNames = userRoles.Select(ur => ur.Role.Name).ToList();

            var comment = await _unitOfWork.GetRepository<Comment>().GetByIdAsync(commentId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bình luận không tồn tại.");

            if (userId != comment.UserId && !roleNames.Contains("Admin") && !roleNames.Contains("Staff"))
            {
                throw new ErrorException(StatusCodes.Status403Forbidden, ResponseCodeConstants.FORBIDDEN, "Bạn không có quyền sửa bình luận này.");
            }

            if (comment.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Bình luận đã bị xóa.");
            }

            var replies = await _unitOfWork.GetRepository<Comment>().Entities
                .Where(c => c.ParentCommentId == commentId && !c.DeletedTime.HasValue)
                .ToListAsync();

            if (replies.Any())
            {
                comment.Content = "Bình luận đã bị xóa";
                comment.LastUpdatedTime = CoreHelper.SystemTimeNow;
                comment.LastUpdatedBy = userId.ToString();
            }
            else
            {
                comment.LastUpdatedTime = CoreHelper.SystemTimeNow;
                comment.LastUpdatedBy = userId.ToString();
                comment.DeletedTime = CoreHelper.SystemTimeNow;
                comment.DeletedBy = userId.ToString();
            }

            await _unitOfWork.GetRepository<Comment>().UpdateAsync(comment);
            await _unitOfWork.SaveAsync();
        }
    }
}
