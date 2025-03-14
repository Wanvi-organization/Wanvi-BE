using Wanvi.Contract.Repositories.Entities;

namespace Wanvi.ModelViews.MediaModelViews
{
    public class ResponseMediaModel
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public string? AltText { get; set; }
    }
}
