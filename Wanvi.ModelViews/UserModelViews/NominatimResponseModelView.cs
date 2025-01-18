using System.Text.Json.Serialization;

namespace Wanvi.ModelViews.UserModelViews
{
    public class NominatimResponseModelView
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; }

        [JsonPropertyName("lon")]
        public string Lon { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
    }
}
