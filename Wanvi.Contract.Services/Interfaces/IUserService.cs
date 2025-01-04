using Wanvi.ModelViews.UserModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IUserService
    {
        Task<IList<UserResponseModel>> GetAll();
    }
}
