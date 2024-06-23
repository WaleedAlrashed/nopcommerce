using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace Nop.Plugin.Payments.Tamara
{
    public class TamaraDefault
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Payments.Tamara";

        /// <summary>
        /// Gets the user agent used to request third-party services
        /// </summary>
        public static string UserAgent => $"nopCommerce-{NopVersion.CURRENT_VERSION}";

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Payments.Tamara.Configure";

        /// <summary>
        /// Gets the webhook route name
        /// </summary>
        public static string WebhookRouteName => "Plugin.Payments.Tamara.Webhook";


        /// <summary>
        /// Gets the one page checkout route name
        /// </summary>
        public static string OnePageCheckoutRouteName => "CheckoutOnePage";

        /// <summary>
        /// Gets the shopping cart route name
        /// </summary>
        public static string ShoppingCartRouteName => "ShoppingCart";

        /// <summary>
        /// Gets the session key to get process payment request
        /// </summary>
        public static string PaymentRequestSessionKey => "OrderPaymentInfo";

        /// <summary>
        /// Gets the base URL of onboarding services
        /// </summary>
        public static string ServiceUrl => "https://api-sandbox.tamara.co/";

        //public static string ServiceUrl => "https://api.tamara.co/";

        /// <summary>
        /// Gets a period (in seconds) before the request times out
        /// </summary>
        public static int RequestTimeout => 200;

        /// <summary>
        /// Gets webhook url
        /// </summary>
        public static Dictionary<string, string> MerchantUrls => new()
        {
            { "Success","TamaraWebhook/Success"},
            { "Failure","TamaraWebhook/Failure"},
            { "Cancel","TamaraWebhook/Cancel"},
            { "Notification","TamaraWebhook/Notification"},
        };
    }
}
