using Wanvi.ModelViews.PostModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<ResponsePostModel>> GetAllPostsAsync();
        Task<IEnumerable<ResponsePostModel>> GetPostsByUserIdAsync(Guid userId);
        Task<ResponsePostModel> GetPostByIdAsync(string id);
        Task CreatePostAsync(CreatePostModel model);
        Task UpdatePostAsync(string id, UpdatePostModel model);
        Task DeletePostAsync(string id);
        Task LikePostAsync(string id);
        Task UnlikePostAsync(string id);
    }
}
