using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tabby.Domain.RequestParams
{
    public class ProductParams
    {
        public ProductParams(int referenceId, string type, string name, string sku, int quantity, AmountParams totalAmount)
        {
            ReferenceId = referenceId;
            Type = type;
            Name = name;
            Sku = sku;
            Quantity = quantity;
            TotalAmount = totalAmount;
        }

        /// <summary>
        /// Gets or sets reference_id
        /// </summary>
        [JsonProperty(PropertyName = "reference_id")]
        public int ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets type
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets sku
        /// </summary>
        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets quantity
        /// </summary>
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets total amount
        /// </summary>
        [JsonProperty(PropertyName = "total_amount")]
        public AmountParams TotalAmount { get; set; }
    }
}
