using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tabby.Domain.Requests
{
    public class RegisterWebhookRequest: Request
    {
        public RegisterWebhookRequest(string url)
        {
            Url = url;
            Events = new string[] { "order_approved", "order_on_hold", "order_declined", "order_authorised", "order_canceled", "order_captured", "order_refunded", "order_expired" };
        }
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
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Path => $"webhooks";

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Method => HttpMethods.Post;
    }
}
