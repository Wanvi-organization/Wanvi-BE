using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanvi.ModelViews.BookingModelViews;
using Wanvi.ModelViews.PaymentModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePayOSPaymentAllLink(CreatePayOSPaymentRequest request);
        Task<string> CreatePayOSPaymentHaftLink(CreatePayOSPaymentRequest request);
        //bool VerifyPayOSSignature(PayOSWebhookRequest request, string signature);
        Task PayOSCallback(PayOSWebhookRequest request);
        Task<string> CreateBookingHaftEnd(CreateBookingEndModel model);
    }
}
