using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.PaymentModelViews
{
    public class PayOSWebhookRequest
    {
        public string paymentId { get; set; }
        public string status { get; set; }
        public long amount { get; set; }
        public int orderCode { get; set; }
        public string description { get; set; }
        public string signature { get; set; }
        // ... Các trường dữ liệu khác (nếu có) ...
    }
}
