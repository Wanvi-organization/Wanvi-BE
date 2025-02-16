using System.Text.Json.Serialization;

namespace Wanvi.ModelViews.VietMapModelViews
{
    public class ResponsePlaceModel
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }
}
