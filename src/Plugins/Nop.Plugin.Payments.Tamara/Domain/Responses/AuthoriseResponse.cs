using System;
using Newtonsoft.Json;
using Nop.Plugin.Payments.Tamara.Domain.RequestParams;

namespace Nop.Plugin.Payments.Tamara.Domain.Responses
{
    public class AuthoriseResponse: Response
    {
        /// <summary>
        /// Gets or sets capture id
        /// </summary>
        [JsonProperty(PropertyName = "capture_id")]
        public string CaptureId { get; set; }

        /// <summary>
        /// Gets or sets status / PAY_BY_INSTALMENTS or PAY_NOW
        /// </summary>
        [JsonProperty(PropertyName = "payment_type")]
        public string PaymentType { get; set; }

        /// <summary>
        /// Gets or sets authorized amount
        /// </summary>
        [JsonProperty(PropertyName = "authorized_amount")]
        public AmountParams AuthorizedAmount { get; set; }

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
    }
}
