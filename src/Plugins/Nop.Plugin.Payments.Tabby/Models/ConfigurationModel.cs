using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Tabby.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Payments.Tabby.Fields.PublicKey")]
        public string PublicKey { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Payments.Tabby.Fields.SecretKey")]
        [DataType(DataType.Password)]
        public string SecretKey { get; set; } = string.Empty;

        #endregion
    }
}