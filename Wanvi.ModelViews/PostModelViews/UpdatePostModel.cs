namespace Wanvi.ModelViews.PostModelViews
{
    public class UpdatePostModel
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<string>? Hashtags { get; set; }
        public List<string>? MediaUrls { get; set; }
    }
}
