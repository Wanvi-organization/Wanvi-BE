using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Tour : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double HourlyRate { get; set; }
        public string PickupAddressId { get; set; }
        public virtual Address PickupAddress { get; set; }
        public string DropoffAddressId { get; set; }
        public virtual Address DropoffAddress { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<TourAddress> TourAddresses { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<Media> Medias { get; set; }
        public virtual ICollection<TourActivity> TourActivities { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
