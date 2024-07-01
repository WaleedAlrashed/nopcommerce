using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Tabby
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class TabbySettings : ISettings
    {
        /// <summary>
        /// Gets or sets public key
        /// </summary>
        public string PublicKey { get; set; } = "pk_test_80d3109b-b620-4121-bb99-02cb63faef76";

        /// <summary>
        /// Gets or sets secret key
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a webhook ID
        /// </summary>
        public string WebhookId { get; set; }
    }
}