using Wanvi.ModelViews.MediaModelViews;
using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.ModelViews.TourModelViews
{
    public class ResponseTourWithIdModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double HourlyRate { get; set; }
        public string PickupAddress { get; set; }
        public string DropoffAddress { get; set; }
        public string LocalGuideId { get; set; }
        public string LocalGuideName { get; set; }
        public string Note { get; set; }
        public List<string> TourAddresses { get; set; }
        public List<ResponseScheduleModel> Schedules { get; set; }
        public List<ResponseMediaModel> Medias { get; set; }
        public List<string> TourActivities { get; set; }
    }
}
