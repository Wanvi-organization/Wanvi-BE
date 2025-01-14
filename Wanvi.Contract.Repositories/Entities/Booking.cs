using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled,
        Refunded
    }

    public class Booking : BaseEntity
    {
        public BookingStatus Status { get; set; }
        public int TotalTravelers { get; set; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }

        public string ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
        public virtual ICollection<BookingPayment> BookingPayments { get; set; }
    }
}
