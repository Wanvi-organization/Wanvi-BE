using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class GetBookingUserDetailModel
    {
        public string ScheduleId { get; set; }
        public int TotalTravelers { get; set; }
        public int TotalTravelersOfTour { get; set; }
        public string RentalDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TourName { get; set; }

    }
}
