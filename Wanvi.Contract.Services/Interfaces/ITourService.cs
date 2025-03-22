using Wanvi.ModelViews.TourModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ITourService
    {
        Task<IEnumerable<ResponseTourModel>> GetAllAsync();
        Task<IEnumerable<ResponseTourModel>> GetAllByLocalGuideId(Guid userId);
        Task<ResponseTourModel> GetByIdAsync(string id);
        Task<ResponseTourWithIdModel> GetByIdWithIdsAsync(string id);
        Task<List<PopularTourModel>> GetPopularToursByCity(string cityId, string? day, string? month, int? year, string? startDate, string? endDate);
        Task<TotalTourStatisticsModel> GetTourCitySummary(string? day, string? month, int? year, string? startDate, string? endDate);
        Task CreateAsync(CreateTourModel model);
        Task UpdateAsync(string id, UpdateTourModel model);
        Task DeleteAsync(string id);
    }
}
