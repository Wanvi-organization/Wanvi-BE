using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Address : BaseEntity
    {
        public string Street { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string DistrictId { get; set; }
        public virtual District District { get; set; }
        public virtual ICollection<Tour> PickupTours { get; set; }
        public virtual  ICollection<Tour> DropoffTours { get; set; }
        public virtual ICollection<TourAddress> TourPoints { get; set; }
    }
}
