using Newtonsoft.Json;
using Nop.Plugin.Payments.Tabby.Domain.RequestParams;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.Tabby.Domain.Responses
{
    public class OrderDetailsResponse: Response
    {
        /// <summary>
        /// Gets or sets Tabby id
        /// </summary>
        [JsonProperty(PropertyName = "order_number")]
        public string OrderNumber { get; set; }

        /// <summary>
        /// Gets or sets order reference id
        /// </summary>
        [JsonProperty(PropertyName = "order_reference_id")]
        public string OrderReferenceId { get; set; }

        /// <summary>
        /// Gets or sets description
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets country code
        /// </summary>
        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }
        /// <summary>
        /// Gets or sets payment type
        /// </summary>
        [JsonProperty(PropertyName = "payment_type")]
        public string PaymentType { get; set; }

        /// <summary>
        /// Gets or sets total amount
        /// </summary>
        [JsonProperty(PropertyName = "total_amount")]
        public AmountParams TotalAmount { get; set; }
        /// <summary>
        /// Gets or sets items
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        public List<ProductParams> Items { get; set; }

        /// <summary>
        /// Gets or sets consumer
        /// </summary>
        [JsonProperty(PropertyName = "consumer")]
        public ConsumerParams Consumer { get; set; }

        /// <summary>
        /// Gets or sets shipping address
        /// </summary>
        [JsonProperty(PropertyName = "shipping_address")]
        public ShippingAddressParams ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets shipping amount
        /// </summary>
        [JsonProperty(PropertyName = "shipping_amount")]
        public AmountParams ShippingAmount { get; set; }

        /// <summary>
        /// Gets or sets tax amount
        /// </summary>
        [JsonProperty(PropertyName = "tax_amount")]
        public AmountParams TaxAmount { get; set; }
    }
}
