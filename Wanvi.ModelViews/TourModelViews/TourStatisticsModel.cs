namespace Wanvi.ModelViews.TourModelViews
{
    public class TourStatisticsModel
    {
        public string TimePeriod { get; set; }
        public int TotalTours { get; set; }
        public int TotalByArea { get; set; }
        public List<PopularTourModel> PopularTours { get; set; }
    }
}
