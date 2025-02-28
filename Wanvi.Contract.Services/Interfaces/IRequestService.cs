using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.RequestModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IRequestService
    {
        Task<IEnumerable<ResponseRequestModel>> GetAllAsync(RequestStatus? status = null);
        Task<ResponseRequestModel> GetByIdAsync(string id);
    }
}
