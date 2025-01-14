using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class BookingDetail : BaseEntity
    {
        public string BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        public string TravelerName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public int Age { get; set; }
        public string? IdentityCard { get; set; }
        public string? PassportNumber { get; set; }
    }
}
