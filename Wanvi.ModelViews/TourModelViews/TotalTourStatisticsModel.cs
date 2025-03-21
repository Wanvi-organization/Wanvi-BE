namespace Wanvi.ModelViews.TourModelViews
{
    public class TotalTourStatisticsModel
    {
        public string TimePeriod { get; set; }
        public int TotalTours { get; set; }
        public List<CityTourStatisticsModel> CityStatistics { get; set; }
    }
}
