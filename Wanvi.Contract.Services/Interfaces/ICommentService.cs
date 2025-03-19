using Wanvi.ModelViews.CommentModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<ResponseCommentModel>> GetAllCommentsByNewsIdAsync(string newsId);
        Task CreateNewsCommentAsync(string newsId, CreateCommentModel model);
        Task ReplyCommentAsync(string commentId, CreateCommentModel model);
        Task<bool> LikeUnlikeCommentAsync(string commentId);
        Task UpdateCommentAsync(string commentId, UpdateCommentModel model);
        Task DeleteCommentAsync(string commentId);
    }
}
