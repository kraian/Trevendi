using Newtonsoft.Json;
using System.Collections.Generic;

namespace Web.Models
{
    public class Payee
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal Total { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Item> Items { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Reference { get; set; }
    }
}
