using Newtonsoft.Json;

namespace Web.Models
{
    public class ArcadierToken
    {
        [JsonProperty(PropertyName = "access_token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "UserId")]
        public string UserId { get; set; }
    }
}
