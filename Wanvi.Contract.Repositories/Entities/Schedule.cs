using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public class Schedule : BaseEntity
    {
        public Core.Constants.Enum.DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxTraveler { get; set; }
        public int BookedTraveler { get; set; } = 0;

        public string TourId { get; set; }
        public virtual Tour Tour { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
