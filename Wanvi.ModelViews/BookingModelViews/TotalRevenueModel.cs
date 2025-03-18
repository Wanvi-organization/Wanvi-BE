using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class TotalRevenueModel
    {
        public string Time {  get; set; }
        public long CommissionRevenue { get; set; }
        public long TourGuideRevenue { get; set; }
    }
}
