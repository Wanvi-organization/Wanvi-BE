namespace Wanvi.Contract.Repositories.Entities
{
    public class BookingPayment
    {
        public string BookingId { get; set; }
        public Booking Booking { get; set; }

        public string PaymentId { get; set; }
        public Payment Payment { get; set; }
    }
}
