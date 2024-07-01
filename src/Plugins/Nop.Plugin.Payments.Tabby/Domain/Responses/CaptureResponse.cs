using Newtonsoft.Json;
using Nop.Plugin.Payments.Tabby.Domain.RequestParams;

namespace Nop.Plugin.Payments.Tabby.Domain.Responses
{
    public class CaptureResponse
    {
        /// <summary>
        /// Gets or sets order id
        /// </summary>
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets capture id
        /// </summary>
        [JsonProperty(PropertyName = "capture_id")]
        public string CaptureId { get; set; }

        /// <summary>
        /// Gets or sets status / fully_captured or partially_captured
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets captured amount
        /// </summary>
        [JsonProperty(PropertyName = "captured_amount")]
        public AmountParams CapturedAmount { get; set; }
    }
}
