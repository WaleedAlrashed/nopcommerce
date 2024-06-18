using Nop.Core.Configuration;
using Nop.Plugin.Payments.Tamara.Domain;

namespace Nop.Plugin.Payments.Tamara;

/// <summary>
/// Represents plugin settings
/// </summary>
public class TamaraSettings : ISettings
{


    /// <summary>
    /// Gets or sets a value indicating whether to manually set the credentials.
    /// For example, there is already an app created, or if the merchant wants to use the sandbox mode.
    /// </summary>
    public bool SetCredentialsManually { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use sandbox environment
    /// </summary>
    public bool UseSandbox { get; set; }

    /// <summary>
    /// Gets or sets client secret
    /// </summary>
    public string PublicKey { get; set; }

    /// <summary>
    /// Gets or sets client secret
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the payment type
    /// </summary>
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// Gets or sets a webhook URL
    /// </summary>
    public string WebhookUrl { get; set; }

}