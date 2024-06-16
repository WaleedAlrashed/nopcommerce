using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Tamara.Models;

/// <summary>
/// Represents configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region Ctor

    public ConfigurationModel()
    {
        PaymentTypes = new List<SelectListItem>();
        OnboardingModel = new OnboardingModel();
    }

    #endregion

    #region Properties

    public bool IsConfigured { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.Email")]
    [EmailAddress]
    public string Email { get; set; }
    public bool Email_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.SetCredentialsManually")]
    public bool SetCredentialsManually { get; set; }
    public bool SetCredentialsManually_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.UseSandbox")]
    public bool UseSandbox { get; set; }
    public bool UseSandbox_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.ClientId")]
    public string ClientId { get; set; }
    public bool ClientId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.SecretKey")]
    [NoTrim]
    [DataType(DataType.Password)]
    public string SecretKey { get; set; }
    public bool SecretKey_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Tamara.Fields.PaymentType")]
    public int PaymentTypeId { get; set; }
    public bool PaymentTypeId_OverrideForStore { get; set; }
    public IList<SelectListItem> PaymentTypes { get; set; }

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

    public OnboardingModel OnboardingModel { get; set; }

    #endregion
}