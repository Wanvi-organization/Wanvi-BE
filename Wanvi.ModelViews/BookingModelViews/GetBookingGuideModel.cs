using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class GetBookingGuideModel
    {
        public string TourName { get; set; }
        public int TotalBooking { get; set; }
        public long TotailPrice { get; set; }
        public string RentalDate { get; set; }
        public List<GetBookingUserByTourGuideModel> Bookings { get; set; } = new();
    }
}
