namespace Wanvi.ModelViews.ReviewModelViews
{
    public class ResponseReviewModel
    {
        public enum ReviewType
        {
            TourReview = 0,
            LocalGuideReview = 1
        }

        public string Id { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public Guid TravelerId { get; set; }
        public string TravelerName { get; set; }
        public string? TourId { get; set; }
        public string? TourName { get; set; }
        public string? BookingId { get; set; }
        public string LocalGuideId { get; set; }
        public string LocalGuideName { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public List<string> MediaUrls { get; set; } = new List<string>();
    }
}
