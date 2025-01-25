namespace Wanvi.ModelViews.UserModelViews
{
    public class ResponseLocalGuideModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public double AvgRating { get; set; }
        public int ReviewCount { get; set; }
        public string Address { get; set; }
        public double MinHourlyRate { get; set; }
        public double Distance { get; set; }
        public bool IsPremium { get; set; }
        public bool IsVerified { get; set; }
    }
}