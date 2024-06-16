using FluentValidation;
using Nop.Plugin.Payments.Tamara.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Tamara.Validators;

/// <summary>
/// Represents onboarding model validator
/// </summary>
public class OnboardingValidator : BaseNopValidator<OnboardingModel>
{
    #region Ctor

    public OnboardingValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.Email)
            .NotEmpty()
            .IsEmailAddress()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.Common.WrongEmail"));
    }

    #endregion
}