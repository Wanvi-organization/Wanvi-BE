using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.TourModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ITourService
    {
        Task<Tour> CreateTourAsync(CreateTourModel model);
    }
}
