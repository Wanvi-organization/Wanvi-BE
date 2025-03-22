namespace Wanvi.ModelViews.ReviewModelViews
{
    public class ResponseTourReviewModel
    {
        public string Id { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public Guid TravelerId { get; set; }
        public string TravelerName { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public List<string> MediaUrls { get; set; } = new List<string>();
    }
}
