using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum PaymentMethod
    {
        EWallet,
        Banking
    }

    public enum PaymentStatus
    {
        Unpaid = 0,
        Paid = 1,
        Refunded = 2,
        Canceled = 3,
        UnpaidRecharge = 4,
        Recharged = 5,
    }

    public class Payment : BaseEntity
    {
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public int OrderCode { get; set; }
        public double Amount { get; set; }
        public string? Description { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public string? BuyerAddress { get; set; }
        public string? Signature { get; set; }
        public string? BookingId { get; set; }
        public virtual Booking Booking { get; set; }
    }
}
