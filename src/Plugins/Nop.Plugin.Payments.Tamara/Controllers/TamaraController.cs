using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Tamara.Models;
using Nop.Plugin.Payments.Tamara.Services;

using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Tamara.Controllers;

[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
[ValidateIpAddress]
[AuthorizeAdmin]
public class TamaraController : BasePluginController
{
    #region Fields

    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;
    protected readonly IWorkContext _workContext;
    protected readonly ServiceManager _serviceManager;
    protected readonly ShoppingCartSettings _shoppingCartSettings;

    #endregion

    #region Ctor

    public TamaraController(IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext,
        IWorkContext workContext,
        ServiceManager serviceManager,
        ShoppingCartSettings shoppingCartSettings)
    {
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
        _workContext = workContext;
        _serviceManager = serviceManager;
        _shoppingCartSettings = shoppingCartSettings;
    }

    #endregion



    #region Methods

    public async Task<IActionResult> Configure(bool showtour = false)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<TamaraSettings>(storeId);

        //we don't need some of the shared settings that loaded above, so load them separately for chosen store
        if (storeId > 0)
        {
            //settings.WebhookUrl = await _settingService
            //    .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.WebhookUrl)}", storeId: storeId);
            //settings.UseSandbox = await _settingService
            //    .GetSettingByKeyAsync<bool>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.UseSandbox)}", storeId: storeId);
            settings.PublicKey = await _settingService
                .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.PublicKey)}", storeId: storeId);
            settings.SecretKey = await _settingService
                .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.SecretKey)}", storeId: storeId);
            //settings.Email = await _settingService
            //    .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.Email)}", storeId: storeId);
            //settings.MerchantGuid = await _settingService
            //    .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.MerchantGuid)}", storeId: storeId);
            //settings.SignUpUrl = await _settingService
            //    .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.SignUpUrl)}", storeId: storeId);
        }

        var model = new ConfigurationModel
        {

            SetCredentialsManually = settings.SetCredentialsManually,
            UseSandbox = settings.UseSandbox,

            SecretKey = settings.SetCredentialsManually ? settings.SecretKey : string.Empty,
            ActiveStoreScopeConfiguration = storeId,
            IsConfigured = ServiceManager.IsConfigured(settings)
        };


        if (storeId > 0)
        {
            model.SetCredentialsManually_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.SetCredentialsManually, storeId);
            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.UseSandbox, storeId);

            model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.SecretKey, storeId);

        }



        //prices and total aren't rounded, so display warning
        if (model.IsConfigured && !_shoppingCartSettings.RoundPricesDuringCalculation)
        {
            var url = Url.Action("AllSettings", "Setting", new { settingName = nameof(ShoppingCartSettings.RoundPricesDuringCalculation) });
            var warning = string.Format(await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.RoundingWarning"), url);
            _notificationService.WarningNotification(warning, false);
        }

        //ensure credentials are valid
        if (!string.IsNullOrEmpty(settings.PublicKey) && !string.IsNullOrEmpty(settings.SecretKey))
        {

            //if (!string.IsNullOrEmpty(credentialsError))
            //    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.Credentials.Invalid"));
            //else
            //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.Credentials.Valid"));
        }

        return View("~/Plugins/Payments.Tamara/Views/Configure.cshtml", model);
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<TamaraSettings>(storeId);

        //set new settings values
        settings.SetCredentialsManually = model.SetCredentialsManually;
        settings.PublicKey = model.PublicKey;
        settings.SecretKey = model.SecretKey;

        if (model.SetCredentialsManually)
        {
            //we don't need some of the shared settings that loaded above, so load them separately for chosen store
            if (storeId > 0)
            {
                settings.WebhookUrl = await _settingService
                    .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.WebhookUrl)}", storeId: storeId);
                settings.UseSandbox = await _settingService
                    .GetSettingByKeyAsync<bool>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.UseSandbox)}", storeId: storeId);
                settings.PublicKey = await _settingService
                    .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.PublicKey)}", storeId: storeId);
                settings.SecretKey = await _settingService
                    .GetSettingByKeyAsync<string>($"{nameof(TamaraSettings)}.{nameof(TamaraSettings.SecretKey)}", storeId: storeId);
            }

            ////first delete the unused webhook on a previous client, if changed
            //if ((!model.ClientId?.Equals(settings.ClientId) ?? true) &&
            //    !string.IsNullOrEmpty(settings.WebhookUrl) &&
            //    !string.IsNullOrEmpty(settings.ClientId) &&
            //    !string.IsNullOrEmpty(settings.SecretKey))
            //{
            //    await _serviceManager.DeleteWebhookAsync(settings);
            //}

            settings.UseSandbox = model.UseSandbox;

            settings.SecretKey = model.SecretKey;



            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.UseSandbox, model.UseSandbox_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.SecretKey, model.SecretKey_OverrideForStore, storeId, false);
        }

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.SetCredentialsManually, model.SetCredentialsManually_OverrideForStore, storeId, false);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }



    #endregion
}