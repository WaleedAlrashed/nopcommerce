using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tabby.Domain.Responses
{
    public class CheckoutResponse
    {
        /// <summary>
        /// Gets or sets order id
        /// </summary>
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets checkout id
        /// </summary>
        [JsonProperty(PropertyName = "checkout_id")]
        public string CheckoutId { get; set; }

        /// <summary>
        /// Gets or sets order id
        /// </summary>
        [JsonProperty(PropertyName = "checkout_url")]
        public string CheckoutUrl { get; set; }
    }
}
