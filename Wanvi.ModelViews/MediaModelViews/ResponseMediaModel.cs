using Wanvi.Contract.Repositories.Entities;

namespace Wanvi.ModelViews.MediaModelViews
{
    public class ResponseMediaModel
    {
        public enum MediaType
        {
            Image,
            Video
        }

        public string Url { get; set; }
        public MediaType Type { get; set; }
        public string? AltText { get; set; }
    }
}
