using System.ComponentModel.DataAnnotations;
using Wanvi.ModelViews.AddressModelViews;
using Wanvi.ModelViews.MediaModelViews;
using Wanvi.ModelViews.ScheduleModelViews;

namespace Wanvi.ModelViews.TourModelViews
{
    public class CreateTourModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Hourly Rate must be a positive number.")]
        public double HourlyRate { get; set; }
        [Required]
        public CreateAddressModel PickupAddress { get; set; }
        [Required]
        public CreateAddressModel DropoffAddress { get; set; }
        [Required]
        public List<CreateAddressModel> TourAddresses { get; set; } = new();
        [Required]
        public List<CreateScheduleModel> Schedules { get; set; } = new();
        [Required]
        public List<CreateMediaModel> Medias { get; set; } = new();
        [Required]
        public List<string> TourActivityIds { get; set; } = new();
    }
}
