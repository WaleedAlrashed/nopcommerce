using System;
using Newtonsoft.Json;
using Nop.Plugin.Payments.Tabby.Domain.RequestParams;

namespace Nop.Plugin.Payments.Tabby.Domain.Responses
{
    public class OrderResponse
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
        /// Gets or sets capture id
        /// </summary>
        [JsonProperty(PropertyName = "capture_id")]
        public string CaptureId { get; set; }

        /// <summary>
        /// Gets or sets status
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets status / PAY_BY_INSTALMENTS or PAY_NOW
        /// </summary>
        [JsonProperty(PropertyName = "payment_type")]
        public string PaymentType { get; set; } = "PAY_BY_INSTALMENTS";

        /// <summary>
        /// Gets or sets authorized amount
        /// </summary>
        [JsonProperty(PropertyName = "authorized_amount")]
        public AmountParams AuthorizedAmount { get; set; }

        /// <summary>
        /// Gets or sets captured amount
        /// </summary>
        [JsonProperty(PropertyName = "captured_amount")]
        public AmountParams CapturedAmount { get; set; }

        /// <summary>
        /// Gets or sets order expiry time
        /// </summary>
        [JsonProperty(PropertyName = "order_expiry_time")]
        public DateTime? OrderExpiryTime { get; set; }

        /// <summary>
        /// Gets or sets auto captured
        /// </summary>
        [JsonProperty(PropertyName = "auto_captured")]
        public bool AutoCaptured { get; set; }

        /// <summary>
        /// Gets or sets order id
        /// </summary>
        [JsonProperty(PropertyName = "checkout_url")]
        public string CheckoutUrl { get; set; }

        /// <summary>
        /// Gets or sets customer id
        /// </summary>
        [JsonIgnore]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets store id
        /// </summary>
        [JsonIgnore]
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets store id
        /// </summary>
        [JsonIgnore]
        public decimal OrderTotal { get; set; }
    }
}
