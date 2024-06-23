using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tabby.Domain.RequestParams
{
    public class AmountParams
    {
        public AmountParams(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        /// <summary>
        /// Gets or sets amount
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets currency
        /// </summary>
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }
}
