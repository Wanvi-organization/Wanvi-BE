using Wanvi.Core.Bases;

namespace Wanvi.Contract.Repositories.Entities
{
    public enum DayOfWeek
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }

    public class Schedule : BaseEntity
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxTraveler { get; set; }
        public int BookedTraveler { get; set; } = 0;
        public double MinDeposit { get; set; }
        public string TourId { get; set; }
        public virtual Tour Tour { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
