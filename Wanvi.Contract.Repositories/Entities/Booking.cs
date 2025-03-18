using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum BookingStatus
    {
        DepositHaft = 0,
        DepositAll = 1,
        DepositedHaft = 2,
        Paid = 3,
        Completed = 4,
        Cancelled = 5,
        Refunded = 6
    }

    public class Booking : BaseEntity
    {
        public BookingStatus Status { get; set; }
        public int TotalTravelers { get; set; }
        public long OrderCode { get; set; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }
        public DateTime RentalDate { get; set; }
        public bool Request {  get; set; }
        public string ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; }
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual Review Review { get; set; }
    }
}
