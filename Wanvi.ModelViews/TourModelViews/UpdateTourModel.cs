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
        public string? PickupAddressId { get; set; }
        public string? DropoffAddressId { get; set; }
        public List<string>? TourAddressIds { get; set; }
        public List<UpdateScheduleModel>? Schedules { get; set; }
        public List<string>? MediaIds { get; set; }
        public List<string>? TourActivityIds { get; set; }
        public string? Note { get; set; }
    }
}
