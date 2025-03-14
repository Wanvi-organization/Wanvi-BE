using Microsoft.AspNetCore.Http;

namespace Wanvi.ModelViews.MediaModelViews
{
    public class UploadMediaModel
    {
        public List<IFormFile> Files { get; set; }
    }
}
