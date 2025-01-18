namespace Wanvi.ModelViews.BookingModelViews
{
    public class GoogleGeocodingModelView
    {
        public string Status { get; set; }
        public IEnumerable<Result> Results { get; set; }
    }

    public class Result
    {
        public Geometry Geometry { get; set; }
    }

    public class Geometry
    {
        public Location Location { get; set; }
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
