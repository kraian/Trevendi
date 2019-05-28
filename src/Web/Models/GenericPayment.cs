using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class GenericPayment
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MarketplaceUrl { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Payee> PayeeInfos { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InvoiceNo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PayKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Total { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Gateway { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Hashkey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? AgreedDateTime { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Note { get; set; }
    }
}
