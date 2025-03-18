using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class BookingStatisticsModel
    {
        public int TotalBooking { get; set; }
        public int TotalCompleted {  get; set; }
        public int TotalCancelled { get; set; }
    }
}
