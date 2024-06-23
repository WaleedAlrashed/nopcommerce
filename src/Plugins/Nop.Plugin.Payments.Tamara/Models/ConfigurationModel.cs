
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Tamara.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        #region Properties
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.ApiToken")]
        public string ApiToken { get; set; }
        public bool ApiToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.DisplayButtonsOnShoppingCart")]
        public bool DisplayButtonsOnShoppingCart { get; set; }
        public bool DisplayButtonsOnShoppingCart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.DisplayButtonsOnProductDetails")]
        public bool DisplayButtonsOnProductDetails { get; set; }
        public bool DisplayButtonsOnProductDetails_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.DisplayLogoInHeaderLinks")]
        public bool DisplayLogoInHeaderLinks { get; set; }
        public bool DisplayLogoInHeaderLinks_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.LogoInHeaderLinks")]
        public string LogoInHeaderLinks { get; set; }
        public bool LogoInHeaderLinks_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.DisplayLogoInFooter")]
        public bool DisplayLogoInFooter { get; set; }
        public bool DisplayLogoInFooter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.LogoInFooter")]
        public string LogoInFooter { get; set; }
        public bool LogoInFooter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.DisplayPayLaterMessages")]
        public bool DisplayPayLaterMessages { get; set; }
        public bool DisplayPayLaterMessages_OverrideForStore { get; set; }

        #endregion

    }
}
