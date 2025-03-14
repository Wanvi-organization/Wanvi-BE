using Wanvi.ModelViews.HashtagModelViews;

namespace Wanvi.ModelViews.PostModelViews
{
    public class CreatePostModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<CreateHashtagModel> Hashtags { get; set; }
        public List<string> MediaUrls { get; set; }
    }
}
