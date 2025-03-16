namespace Wanvi.ModelViews.PostModelViews
{
    public class ResponsePostModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByUser { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> Hashtags { get; set; }
        public List<string> MediaUrls { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
    }
}
