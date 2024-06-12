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
        /// Gets the service js script URL
        /// </summary>
        public static string ServiceScriptUrl => "https://checkout.tabby.ai/tabby-pg.js";

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
        /// Gets a list of currencies that do not support decimals.
        /// </summary>
        public static List<string> CurrenciesWithoutDecimals => new List<string> { "JPY", "KRW", "VND" };

        #region Onboarding

        /// <summary>
        /// Represents onboarding constants
        /// </summary>
        public class Onboarding
        {
            /// <summary>
            /// Gets the base URL of onboarding services
            /// </summary>
            public static string ServiceUrl => "https://onboarding.tabby.ai/";

            /// <summary>
            /// Gets the onboarding js script URL
            /// </summary>
            public static string ScriptUrl => "https://sandbox.tabby.ai/onboarding.js";

            /// <summary>
            /// Gets a period (in seconds) before the onboarding request times out
            /// </summary>
            public static int RequestTimeout => 20;
        }

        #endregion
    }
}
