using Wanvi.Contract.Repositories.Entities;

namespace Wanvi.ModelViews.ScheduleModelViews
{
    public class ResponseScheduleModel
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
        public string Id { get; set; }
        public string Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxTraveler { get; set; }
        public int BookedTraveler { get; set; }
        public int RemainingTraveler {  get; set; }
        public double MinDeposit { get; set; }
        public string TourId { get; set; }
    }
}
