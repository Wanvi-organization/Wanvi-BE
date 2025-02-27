using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Wanvi.Core.Constants.Enum;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class WithdrawMoneyFromBookingModel
    {
        public string BookingId { get; set; }
        public string? Note { get; set; }
    }
}
