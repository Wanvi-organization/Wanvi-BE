using Wanvi.ModelViews.MediaModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IMediaService
    {
        Task<IEnumerable<ResponseMediaModel>> UploadAsync(UploadMediaModel model);
        Task DeleteAsync(string id);
    }
}
