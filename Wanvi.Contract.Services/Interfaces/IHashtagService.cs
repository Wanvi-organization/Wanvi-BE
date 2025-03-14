using Wanvi.ModelViews.HashtagModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IHashtagService
    {
        Task<IEnumerable<ResponseHashtagModel>> GetAllAsync();
        Task<ResponseHashtagModel> GetByIdAsync(string id);
        Task CreateAsync(CreateHashtagModel model);
        Task UpdateAsync(string id, UpdateHashtagModel model);
        Task DeleteAsync(string id);
    }
}
