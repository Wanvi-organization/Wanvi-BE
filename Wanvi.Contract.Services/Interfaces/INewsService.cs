using Wanvi.ModelViews.NewsModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<ResponseNewsModel>> GetAllAsync();
        Task<ResponseNewsModel> GetByIdAsync(string id);
        Task CreateAsync(CreateNewsModel model);
        //Task UpdateAsync(string id, UpdateNewsModel model);
        //Task DeleteAsync(string id);
    }
}
