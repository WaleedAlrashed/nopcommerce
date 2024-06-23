using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tamara.Domain.Responses
{
    public class WebhookResponse
    {
        /// <summary>
        /// Gets or sets merchant id
        /// </summary>
        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets webhook id
        /// </summary>
        [JsonProperty(PropertyName = "webhook_id")]
        public string WebhookId { get; set; }

        /// <summary>
        /// Gets or sets url
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets events
        /// </summary>
        [JsonProperty(PropertyName = "events")]
        public string[] Events { get; set; }

        /// <summary>
        /// Gets or sets webhook URL
        /// </summary>
        [JsonIgnore]
        public string WebhookUrl { get; set; }
    }
}
