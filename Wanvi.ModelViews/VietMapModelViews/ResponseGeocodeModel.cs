using System.Text.Json.Serialization;

namespace Wanvi.ModelViews.VietMapModelViews
{
    public class ResponseGeocodeModel
    {
        [JsonPropertyName("ref_id")]
        public string RefId { get; set; }
    }
}
