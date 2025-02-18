using Wanvi.Core.Bases;
using static Wanvi.Core.Constants.Enum;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Payment : BaseEntity
    {
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public double Amount { get; set; }
        public virtual ICollection<BookingPayment> BookingPayments { get; set; }
    }
}
