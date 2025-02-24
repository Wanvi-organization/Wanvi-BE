using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class GetBookingUsermodel
    {
        public string Id { get; set; }
        public int TotalTravelers { get; set; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }
        public string RentalDate { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

    }
}
