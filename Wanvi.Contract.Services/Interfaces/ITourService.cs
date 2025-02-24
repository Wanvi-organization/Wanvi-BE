using Wanvi.ModelViews.TourModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ITourService
    {
        Task<IEnumerable<ResponseTourModel>> GetAllAsync();
        Task<IEnumerable<ResponseTourModel>> GetAllByLocalGuideId(Guid userId);
        Task<ResponseTourModel> GetByIdAsync(string id);
        Task CreateAsync(CreateTourModel model);
        Task UpdateAsync(string id, UpdateTourModel model);
        Task DeleteAsync(string id);
    }
}
