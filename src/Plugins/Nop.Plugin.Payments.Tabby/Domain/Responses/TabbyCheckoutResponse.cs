using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tabby.Domain.Responses
{

    public class TabbyCheckoutResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("configuration")]
        public Configuration Configuration { get; set; }
    }

    public class Configuration
    {
        [JsonProperty("available_products")]
        public AvailableProducts AvailableProducts { get; set; }
    }

    public class AvailableProducts
    {
        [JsonProperty("installments")]
        public List<Installment> Installments { get; set; }
    }

    public class Installment
    {
        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
    }

}

