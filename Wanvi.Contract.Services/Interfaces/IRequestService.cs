using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.RequestModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IRequestService
    {
        Task<IEnumerable<ResponseRequestModel>> GetAllAsync(RequestStatus? status = null, RequestType? type = null);
        Task<ResponseRequestModel> GetByIdAsync(string id);
        Task<string> AccecptFromAdmin(AccecptRequestFromAdminModel model);
        Task<string> CancelFromAdmin(CancelRequestFromAdminModel model);
        Task<string> CreateRequest(CreateRequestModel model);
    }
}
