using Wanvi.ModelViews.CategoryViewModels;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<ResponseCategoryModel>> GetAllAsync();
        Task<ResponseCategoryModel> GetByIdAsync(string id);
        Task CreateAsync(CreateCategoryModel model);
        Task UpdateAsync(string id, UpdateCategoryModel model);
        Task DeleteAsync(string id);
    }
}
