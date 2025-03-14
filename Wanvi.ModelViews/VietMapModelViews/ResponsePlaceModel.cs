using System.Text.Json.Serialization;

namespace Wanvi.ModelViews.VietMapModelViews
{
    public class ResponsePlaceModel
    {
        public string AddressId { get; set; }
        [JsonPropertyName("display")]
        public string Display { get; set; }
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }
}
