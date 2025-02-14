using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanvi.ModelViews.PaymentModelViews;

namespace Wanvi.Contract.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePayOSPaymentLink(CreatePayOSPaymentRequest request);
    }
}
