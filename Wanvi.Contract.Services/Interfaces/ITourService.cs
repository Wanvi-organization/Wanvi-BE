using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.TourModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ITourService
    {
        Task<IEnumerable<ResponseTourModel>> GetAllAsync();
        Task<ResponseTourModel> CreateTourAsync(CreateTourModel model);
    }
}
