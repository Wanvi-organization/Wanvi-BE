using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum PaymentMethod
    {
        EWallet,
        ByCash
    }

    public enum PaymentStatus
    {
        Unpaid,
        Deposited,
        Paid,
        Refunded
    }

    public class Payment : BaseEntity
    {
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public double Amount { get; set; }
        public virtual ICollection<BookingPayment> BookingPayments { get; set; }
    }
}
