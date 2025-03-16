using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class GetBookingUserByTourGuideModel
    {
        public string BookingId { get; set; }
        public string CustomerName { get; set; }
        public int Price { get; set; }
        public string Status { get; set; }
    }
}
