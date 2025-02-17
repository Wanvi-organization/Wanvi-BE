using Wanvi.ModelViews.AddressModelViews;
using Wanvi.ModelViews.MediaModelViews;
using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.ModelViews.TourModelViews
{
    public class UpdateTourModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public double? HourlyRate { get; set; }
        public CreateAddressModel? PickupAddress { get; set; }
        public CreateAddressModel? DropoffAddress { get; set; }
        public List<CreateAddressModel>? TourAddresses { get; set; }
        public List<CreateScheduleModel>? Schedules { get; set; }
        public List<CreateMediaModel>? Medias { get; set; }
        public List<string>? TourActivityIds { get; set; }
    }
}
