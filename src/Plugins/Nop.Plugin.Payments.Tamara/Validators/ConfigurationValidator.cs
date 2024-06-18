using FluentValidation;
using Nop.Plugin.Payments.Tamara.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Tamara.Validators;

/// <summary>
/// Represents configuration model validator
/// </summary>
public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    #region Ctor

    public ConfigurationValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.PublicKey)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Tamara.Fields.PublicKey.Required"))
            .When(model => !model.UseSandbox && model.SetCredentialsManually);

        RuleFor(model => model.SecretKey)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Tamara.Fields.SecretKey.Required"))
            .When(model => !model.UseSandbox && model.SetCredentialsManually);
    }

    #endregion
}