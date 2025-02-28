using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.BookingModelViews
{
    public class CreateBookingModel
    {
        public string TravelerName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime RentalDate { get; set; }
        public int NumberOfParticipants { get; set; }
        public string Note { get; set; }
        public string ScheduleId { get; set; }
    }
}
