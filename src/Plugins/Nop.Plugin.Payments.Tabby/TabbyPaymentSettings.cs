using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Tabby
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class TabbyPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets public key
        /// </summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets secret key
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;
    }
}