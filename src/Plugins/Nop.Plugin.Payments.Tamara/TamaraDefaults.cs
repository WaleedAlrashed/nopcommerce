using Nop.Core;

namespace Nop.Plugin.Payments.Tamara;

/// <summary>
/// Represents plugin constants
/// </summary>
public class TamaraDefaults
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
    /// Gets the nopCommerce partner code
    /// </summary>
    public static string PartnerCode => "NopCommerce_PPCP";

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
    /// Gets the name of a generic attribute to store the refund identifier
    /// </summary>
    public static string RefundIdAttributeName => "TamaraRefundId";

    /// <summary>
    /// Gets the service js script URL
    /// </summary>
    public static string ServiceScriptUrl => "https://www.paypal.com/sdk/js";

    /// <summary>
    /// Gets a default period (in seconds) before the request times out
    /// </summary>
    public static int RequestTimeout => 10;



}