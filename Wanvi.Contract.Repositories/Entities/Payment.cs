using Wanvi.Core.Bases;
using static Wanvi.Core.Constants.Enum;

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
        Deposited = 1,
        Paid = 2,
        Refunded = 3,
        Canceled = 4
    }

    public class Payment : BaseEntity
    {
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public double Amount { get; set; }
        public string BookingId { get; set; }
        public virtual Booking Booking { get; set; }
    }
}
