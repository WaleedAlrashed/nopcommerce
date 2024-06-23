using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tamara.Domain.Responses
{
    /// <summary>
    /// Represents response from the service
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Gets or sets order id
        /// </summary>
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets status / fully_captured or partially_captured
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        
    }
}
