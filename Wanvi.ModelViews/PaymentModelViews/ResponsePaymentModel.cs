using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.PaymentModelViews
{
    public class ResponsePaymentModel
    {
        public string Method { get; set; }
        public string Status { get; set; }
        public long OrderCode { get; set; }
        public double Amount { get; set; }
        public string? Description { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerPhone { get; set; }
        public string? BuyerAddress { get; set; }
        //public string? Signature { get; set; }
        public string? BookingId { get; set; }
    }
}
