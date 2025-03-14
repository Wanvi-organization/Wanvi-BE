using System.Text.Json.Serialization;

namespace Wanvi.ModelViews.VietMapModelViews
{
    public class ResponseAutocompleteModel
    {
        [JsonPropertyName("ref_id")]
        public string RefId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("display")]
        public string Display { get; set; }
    }
}
