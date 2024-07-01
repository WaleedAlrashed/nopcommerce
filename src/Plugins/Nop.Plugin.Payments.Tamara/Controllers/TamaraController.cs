using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Tamara.Models;
using Nop.Plugin.Payments.Tamara.Services;
using Nop.Plugin.Payments.Tamara;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Tamara.Controllers
{
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    [ValidateIpAddress]
    [AuthorizeAdmin]
    public class TamaraController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ServiceManager _serviceManager;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public TamaraController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            ServiceManager serviceManager,
            ShoppingCartSettings shoppingCartSettings)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _serviceManager = serviceManager;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<TamaraSettings>(storeId);

            var model = new ConfigurationModel
            {
                ApiToken = settings.ApiToken,
                DisplayButtonsOnShoppingCart = settings.DisplayButtonsOnShoppingCart,
                DisplayButtonsOnProductDetails = settings.DisplayButtonsOnProductDetails,
                DisplayLogoInHeaderLinks = settings.DisplayLogoInHeaderLinks,
                LogoInHeaderLinks = settings.LogoInHeaderLinks,
                DisplayLogoInFooter = settings.DisplayLogoInFooter,
                DisplayPayLaterMessages = settings.DisplayPayLaterMessages,
                LogoInFooter = settings.LogoInFooter,
            };

            if (storeId > 0)
            {
                model.ApiToken_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.ApiToken, storeId);
                model.DisplayButtonsOnShoppingCart_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.DisplayButtonsOnShoppingCart, storeId);
                model.DisplayButtonsOnProductDetails_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.DisplayButtonsOnProductDetails, storeId);
                model.DisplayLogoInHeaderLinks_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.DisplayLogoInHeaderLinks, storeId);
                model.LogoInHeaderLinks_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.LogoInHeaderLinks, storeId);
                model.DisplayLogoInFooter_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.DisplayLogoInFooter, storeId);
                model.DisplayPayLaterMessages_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.DisplayPayLaterMessages, storeId);
                model.LogoInFooter_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.LogoInFooter, storeId);
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

            settings.ApiToken = model.ApiToken;
            settings.DisplayButtonsOnShoppingCart = model.DisplayButtonsOnShoppingCart;
            settings.DisplayButtonsOnProductDetails = model.DisplayButtonsOnProductDetails;
            settings.DisplayLogoInHeaderLinks = model.DisplayLogoInHeaderLinks;
            settings.LogoInHeaderLinks = model.LogoInHeaderLinks;
            settings.DisplayLogoInFooter = model.DisplayLogoInFooter;
            settings.DisplayPayLaterMessages = model.DisplayPayLaterMessages;
            settings.LogoInFooter = model.LogoInFooter;

            //first delete the unused webhook on a previous client, if changed
            if (!string.IsNullOrEmpty(model.ApiToken) &&
                !string.IsNullOrEmpty(settings.ApiToken) &&
                !model.ApiToken.Equals(settings.ApiToken))
            {
                await _serviceManager.DeleteWebhookAsync(settings);
            }

            if ((!string.IsNullOrEmpty(settings.ApiToken) && !string.IsNullOrEmpty(model.ApiToken) && !model.ApiToken.Equals(settings.ApiToken)) ||
                (string.IsNullOrEmpty(settings.WebhookId) && !string.IsNullOrEmpty(settings.ApiToken)))
            {
                // add webhook
                var (webhook, _) = await _serviceManager.CreateWebhookAsync(settings, storeId);
                settings.WebhookUrl = webhook?.WebhookUrl;
                settings.WebhookId = webhook?.WebhookId;
                if (string.IsNullOrEmpty(settings.WebhookUrl))
                {
                    var url = Url.Action("List", "Log");
                    var warning = string.Format(await _localizationService.GetResourceAsync("Plugins.Payments.PayPalCommerce.WebhookWarning"), url);
                    _notificationService.WarningNotification(warning, false);
                }
            }

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.WebhookId, model.ApiToken_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.WebhookUrl, model.ApiToken_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.ApiToken, model.ApiToken_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.DisplayButtonsOnShoppingCart, model.DisplayButtonsOnShoppingCart_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.DisplayButtonsOnProductDetails, model.DisplayButtonsOnProductDetails_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.DisplayLogoInHeaderLinks, model.DisplayLogoInHeaderLinks_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.LogoInHeaderLinks, model.LogoInHeaderLinks_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.DisplayLogoInFooter, model.DisplayLogoInFooter_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.DisplayPayLaterMessages, model.DisplayPayLaterMessages_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.LogoInFooter, model.LogoInFooter_OverrideForStore, storeId, false);

            await _settingService.ClearCacheAsync();

            return await Configure();

        }
        #endregion

        #region Utilities



        #endregion
    }
}
