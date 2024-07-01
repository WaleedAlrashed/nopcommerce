using Newtonsoft.Json;
using Nop.Plugin.Payments.Tabby.Domain.RequestParams;

namespace Nop.Plugin.Payments.Tabby.Domain.Responses
{
    public class VoidResponse : Response
    {
        /// <summary>
        /// Gets or sets order was voided
        /// </summary>
        [JsonProperty(PropertyName = "order_was_voided")]
        public bool OrderWasVoided { get; set; }

        /// <summary>
        /// Gets or sets captured amount
        /// </summary>
        [JsonProperty(PropertyName = "captured_amount")]
        public AmountParams CapturedAmount { get; set; }
    }
}
