namespace Wanvi.ModelViews.TourModelViews
{
    public class TourStatisticsModel
    {
        public string TimePeriod { get; set; }
        public int TotalTours { get; set; }
        public List<CityTourStatisticsModel> CityStatistics { get; set; }
        public List<PopularTourModel> PopularTours { get; set; }
    }
}
