using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tabby.Domain.Requests
{

    public class TabbyCheckoutRequest : Request
    {
        public TabbyCheckoutRequest()
        {
            Payment = new PaymentParams();
            MerchantUrls = new TabbyMerchantUrlParams();
        }

        [JsonProperty(PropertyName = "payment")]
        public PaymentParams Payment { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Lang { get; set; } = "en";

        [JsonProperty(PropertyName = "merchant_code")]
        public string MerchantCode { get; set; } = TabbyDefaults.MerchantCode;

        [JsonProperty(PropertyName = "merchant_urls")]
        public TabbyMerchantUrlParams MerchantUrls { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Path => "checkout";

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Method => HttpMethods.Post;
    }

    public class PaymentParams
    {
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "buyer")]
        public BuyerParams Buyer { get; set; }

        [JsonProperty(PropertyName = "buyer_history")]
        public BuyerHistoryParams BuyerHistory { get; set; }

        [JsonProperty(PropertyName = "order")]
        public OrderParams Order { get; set; }

        [JsonProperty(PropertyName = "meta")]
        public MetaParams Meta { get; set; }
    }

    public class BuyerParams
    {
        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "dob")]
        public string Dob { get; set; }
    }

    public class BuyerHistoryParams
    {
        [JsonProperty(PropertyName = "registered_since")]
        public string RegisteredSince { get; set; }

        [JsonProperty(PropertyName = "loyalty_level")]
        public int LoyaltyLevel { get; set; }

        [JsonProperty(PropertyName = "wishlist_count")]
        public int WishlistCount { get; set; }

        [JsonProperty(PropertyName = "is_social_networks_connected")]
        public bool IsSocialNetworksConnected { get; set; }

        [JsonProperty(PropertyName = "is_phone_number_verified")]
        public bool IsPhoneNumberVerified { get; set; }

        [JsonProperty(PropertyName = "is_email_verified")]
        public bool IsEmailVerified { get; set; }
    }

    public class OrderParams
    {
        [JsonProperty(PropertyName = "tax_amount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty(PropertyName = "shipping_amount")]
        public decimal ShippingAmount { get; set; }

        [JsonProperty(PropertyName = "discount_amount")]
        public decimal DiscountAmount { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "reference_id")]
        public string ReferenceId { get; set; }
    }

    public class MetaParams
    {
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }
    }

    public class TabbyMerchantUrlParams
    {
        [JsonProperty(PropertyName = "success")]
        public string Success { get; set; } = TabbyDefaults.MerchantUrls["Success"];

        [JsonProperty(PropertyName = "cancel")]
        public string Cancel { get; set; } = TabbyDefaults.MerchantUrls["Cancel"];

        [JsonProperty(PropertyName = "failure")]
        public string Failure { get; set; } = TabbyDefaults.MerchantUrls["Failure"];
    }
}