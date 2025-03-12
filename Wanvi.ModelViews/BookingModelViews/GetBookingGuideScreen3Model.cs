using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class GetBookingGuideScreen3Model
    {
        public string TourName { get; set; }
        public long TotailPrice { get; set; }
        public string RentalDate { get; set; }
        public int TotalCustomer {  get; set; }
        public string CustomerName { get; set; }
        public string DropoffAddress { get; set; }
        public string PickupAddress { get; set; }
        public string Status { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
    }
}
