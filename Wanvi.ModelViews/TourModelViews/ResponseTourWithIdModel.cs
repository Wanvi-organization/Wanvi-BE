using Wanvi.ModelViews.MediaModelViews;
using Wanvi.ModelViews.ReviewModelViews;
using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.ModelViews.TourModelViews
{
    public class ResponseTourWithIdModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double HourlyRate { get; set; }
        public string PickupAddressId { get; set; }
        public string PickupAddress { get; set; }
        public string DropoffAddressId { get; set; }
        public string DropoffAddress { get; set; }
        public string LocalGuideId { get; set; }
        public string LocalGuideName { get; set; }
        public string Note { get; set; }
        public List<ResponseTourAddressModel> TourAddresses { get; set; }
        public List<ResponseScheduleModel> Schedules { get; set; }
        public List<ResponseMediaModel> Medias { get; set; }
        public List<ResponseTourActivityModel> TourActivities { get; set; }
        public List<ResponseTourReviewModel> Reviews { get; set; }
    }
}
