namespace Wanvi.Contract.Repositories.Entities
{
    public class BookingPayment
    {
        public string BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        public string PaymentId { get; set; }
        public virtual Payment Payment { get; set; }
    }
}
