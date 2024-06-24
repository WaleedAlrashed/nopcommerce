using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Tabby.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;


namespace Nop.Plugin.Payments.Tabby.Controllers
{
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    [ValidateIpAddress]
    [AuthorizeAdmin]
    public class TabbyPaymentController : BasePaymentController
    {
        #region Fields

        protected readonly ISettingService _settingService;
        protected readonly IPermissionService _permissionService;
        protected readonly ILocalizationService _localizationService;
        protected readonly INotificationService _notificationService;
        protected readonly IStoreContext _storeContext;

        #endregion


        #region Ctor

        public TabbyPaymentController(ISettingService settingService,
                                      IPermissionService permissionService,
                                      ILocalizationService localizationService,
                                      IStoreContext storeContext,
                                       INotificationService notificationService)
        {
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _notificationService = notificationService;
        }

        #endregion


        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<TabbySettings>(storeScope);

            var model = new ConfigurationModel
            {
                PublicKey = settings.PublicKey,
                SecretKey = settings.SecretKey
            };

            return View("~/Plugins/Payments.Tabby/Views/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<TabbySettings>(storeScope);

            settings.PublicKey = model.PublicKey;
            settings.SecretKey = model.SecretKey;

            await _settingService.SaveSettingAsync(settings);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}
