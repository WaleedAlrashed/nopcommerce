using Nop.Core;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Tabby
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class TabbyDefaults : ISettings
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Payments.Tabby";

        /// <summary>
        /// Gets the user agent used to request third-party services
        /// </summary>
        public static string UserAgent => $"nopCommerce-{NopVersion.CURRENT_VERSION}";

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Payments.Tabby.Configure";

        /// <summary>
        /// Gets the webhook route name
        /// </summary>
        public static string WebhookRouteName => "Plugin.Payments.Tabby.Webhook";

        /// <summary>
        /// Gets the session key to get process payment request
        /// </summary>
        public static string PaymentRequestSessionKey => "OrderPaymentInfo";

        /// <summary>
        /// Gets the name of a generic attribute to store the refund identifier
        /// </summary>
        public static string RefundIdAttributeName => "TabbyRefundId";

        /// <summary>
        /// Gets the checkout service URL
        /// </summary>
        public static string CheckoutUrl => "https://api.tabby.ai/api/v2/checkout";

        /// <summary>
        /// Gets the merchant code
        /// </summary>
        public static string MerchantCode => "KSECRETUAE";

        /// <summary>
        /// Gets a default period (in seconds) before the request times out
        /// </summary>
        public static int RequestTimeout => 10;

        /// <summary>
        /// Gets webhook event names to subscribe
        /// </summary>
        public static List<string> WebhookEventNames => new List<string>
        {
            "PAYMENT.CREATED",
            "PAYMENT.AUTHORIZED",
            "PAYMENT.CAPTURED",
            "PAYMENT.REFUNDED",
            "PAYMENT.VOIDED"
        };

        /// <summary>
        /// Gets webhook url
        /// </summary>
        public static Dictionary<string, string> MerchantUrls => new()
        {
            { "Success","TabbyWebHook/Success"},
            { "Failure","TabbyWebHook/Failure"},
            { "Cancel","TabbyWebHook/Cancel"},
            { "Notification","TabbyWebHook/Notification"},
        };

    }
}
