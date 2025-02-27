using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum BookingStatus
    {
        DepositHaft = 0,
        DepositAll = 1,
        DepositedHaft = 2,
        DepositHaftEnd = 3,
        Paid = 4,
        Completed = 5,
        Cancelled = 6,
        Refunded = 7
    }

    public class Booking : BaseEntity
    {
        public BookingStatus Status { get; set; }
        public int TotalTravelers { get; set; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }
        public DateTime RentalDate { get; set; }
        public string ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
