using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Tabby.Models
{
    /// <summary>
    /// Represents configuration model for Tabby plugin
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor

        public ConfigurationModel()
        {
            // Initialize any lists or complex types here if needed
        }

        #endregion

        #region Properties

        public bool IsConfigured { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tabby.Fields.PublicKey")]
        public string PublicKey { get; set; }
        public bool PublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tabby.Fields.SecretKey")]
        [DataType(DataType.Password)]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        #endregion
    }
}