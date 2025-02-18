using Wanvi.ModelViews.ActivityModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IActivityService
    {
        Task<IEnumerable<ResponseActivityModel>> GetAllAsync();
        Task<ResponseActivityModel> GetByIdAsync(string id);
        Task CreateAsync(CreateActivityModel model);
        Task UpdateAsync(string id, UpdateActivityModel model);
        Task DeleteAsync(string id);
    }
}
