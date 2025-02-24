namespace Wanvi.ModelViews.UserModelViews
{
    public class ResponseLocalGuideProfileModel
    {
        public string FullName { get; set; }
        public bool Gender { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Bio {  get; set; }
        public string Language { get; set; }
        public string PersonalVehicle { get; set; }
        public double AvgRating { get; set; }
        public int ReviewCount { get; set; }
        public int TourCount { get; set; }
        public string Address { get; set; }
        public double MinHourlyRate { get; set; }
        public bool IsVerified { get; set; }
    }
}
