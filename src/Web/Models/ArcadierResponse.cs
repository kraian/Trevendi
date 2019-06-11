using Newtonsoft.Json;

namespace Web.Models
{
    public class ArcadierResponse
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
