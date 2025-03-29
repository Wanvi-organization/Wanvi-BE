using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class GetBookingDetailUserModel
    {
        public string Id { get; set; }
        public int TotalTravelers { get; set; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }
        public string RentalDate { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TourName { get; set; }
        public string TourGuideName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
    }
}
