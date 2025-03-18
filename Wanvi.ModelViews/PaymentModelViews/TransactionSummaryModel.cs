using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanvi.Contract.Repositories.Entities;

namespace Wanvi.ModelViews.PaymentModelViews
{
    public class TransactionSummaryModel
    {
        public int Totalpayment { get; set; }
        public int TotalPaid { get; set; }
        public int TotalCanceled { get; set; }
        public int TotalUnpaidRecharge { get; set; }
        public int TotalRecharged { get; set; }
        public List<ResponsePaymentModel> responsePaymentModels { get; set; }
        
    }
}
