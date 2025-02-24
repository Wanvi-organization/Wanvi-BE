using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class GetBookingGuideModel
    {
        public string TourId { get; set; }
        public int TotalTravelers { get; set; } // Tổng số người đặt tour
        public int MaxTraveler { get; set; } // Giới hạn số người của Schedule
        public int BookedTraveler { get; set; } // Tổng số người đã đặt nhưng chưa hoàn thành
        public string Status { get; set; } // Hoàn thành / Chưa hoàn thành
        public string RentalDate { get; set; }
        public List<GetBookingUsermodel> Bookings { get; set; } = new();
    }
}
