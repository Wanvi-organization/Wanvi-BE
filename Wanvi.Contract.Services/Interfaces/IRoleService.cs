using Wanvi.ModelViews.RoleModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IRoleService
    {
        Task CreateAsync(CreateRoleModel model);
        Task<List<ResponseRoleModel>> GetAllAsync();
        Task UpdateAsync(Guid roleId, UpdateRoleModel model);
        Task DeleteAsync(Guid roleId);
    }
}
