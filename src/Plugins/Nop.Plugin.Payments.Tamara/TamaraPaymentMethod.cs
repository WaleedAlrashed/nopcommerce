using Nop.Core;
using System.Threading.Tasks;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Localization;
using Nop.Services.Configuration;
using Nop.Services.Cms;
using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Payments.Tamara.Services;
using Nop.Plugin.Payments.Tamara.Components;
using Nop.Plugin.Payments.Tamara.Domain;
using Nop.Plugin.Payments.Tamara.Models;
using Nop.Core.Domain.Payments;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Directory;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Stores;
using Nop.Core.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Payments.Tamara
{
    /// <summary>
    /// Rename this file and change to the correct type
    /// </summary>
    public class TamaraPaymentMethod : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields
        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly PaymentSettings _paymentSettings;
        private readonly TamaraSettings _settings;
        private readonly ServiceManager _serviceManager;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public TamaraPaymentMethod(CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            PaymentSettings paymentSettings,
            TamaraSettings settings,
            ServiceManager serviceManager,
            WidgetSettings widgetSettings)
        {
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _paymentSettings = paymentSettings;
            _settings = settings;
            _serviceManager = serviceManager;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;

        #endregion

        #region Methods

        public async override Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new TamaraSettings
            {
                LogoInHeaderLinks = @"<!-- Tamara Logo --><li><a href=""https://support.tamara.co/hc/en-us"" title=""How Tamara Works"" onclick=""javascript:window.open('https://support.tamara.co/hc/en-us','WITamara','toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=yes, resizable=yes, width=1060, height=700'); return false;""><img style=""padding-top:10px;"" src=""https://cdn.salla.sa/qXqEV/5NSVd6hEkYhZvqdeEv3q5A760qtKEFUh4Na1ezMD.png"" border=""0"" alt=""Now accepting Tamara""></a></li><!-- T Logo -->",
                LogoInFooter = @"<!-- Tamara Logo --><div><a href=""https://support.tamara.co/hc/en-us"" title=""How Tamara Works"" onclick=""javascript:window.open('https://support.tamara.co/hc/en-us','WITamara','toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=yes, resizable=yes, width=1060, height=700'); return false;""><img src=""https://cdn.salla.sa/qXqEV/5NSVd6hEkYhZvqdeEv3q5A760qtKEFUh4Na1ezMD.png"" border=""0"" alt=""Tamara Acceptance Mark""></a></div><!-- Tamara Logo -->",
                StyleLayout = "vertical",
                StyleColor = "blue",
                StyleShape = "rect",
                StyleLabel = "tamara",
                DisplayButtonsOnProductDetails = true,
                DisplayButtonsOnShoppingCart = true,
                DisplayPayLaterMessages = false
            });

            await _settingService.SaveSettingAsync(new TamaraSettings
            {
                ApiToken = string.Empty
            });

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.Tamara.Fields.ApiToken"] = "API Token",
                ["Plugins.Payments.Tamara.Payment.PayNow"] = "Pay Now",
                ["Plugins.Payments.Tamara.Prominently"] = "Prominently",
                ["Plugins.Payments.Tamara.TamaraOrderId"] = "Tamara order id",
                ["Plugins.Payments.Tamara.AuthenticationError"] = "Authentication error (error or missing AppToken information)",
                ["Plugins.Payment.Tamara.PaymentMethodDescription"] = "Tamara payment method description",
                ["Plugins.Payments.Tamara.Fields.DisplayButtonsOnProductDetails"] = "Display buttons on product details",
                ["Plugins.Payments.Tamara.Fields.DisplayButtonsOnProductDetails.Hint"] = "Determine whether to display Tamara buttons on product details pages, clicking on them matches the behavior of the default 'Add to cart' button.",
                ["Plugins.Payments.Tamara.Fields.DisplayButtonsOnShoppingCart"] = "Display buttons on shopping cart",
                ["Plugins.Payments.Tamara.Fields.DisplayButtonsOnShoppingCart.Hint"] = "Determine whether to display Tamara buttons on the shopping cart page instead of the default checkout button.",
                ["Plugins.Payments.Tamara.Fields.DisplayLogoInFooter"] = "Display logo in footer",
                ["Plugins.Payments.Tamara.Fields.DisplayLogoInFooter.Hint"] = "Determine whether to display Tamara logo in the footer. These logos and banners are a great way to let your buyers know that you choose Tamara to securely process their payments.",
                ["Plugins.Payments.Tamara.Fields.DisplayLogoInHeaderLinks"] = "Display logo in header links",
                ["Plugins.Payments.Tamara.Fields.DisplayLogoInHeaderLinks.Hint"] = "Determine whether to display Tamara logo in header links. These logos and banners are a great way to let your buyers know that you choose Tamara to securely process their payments.",
                ["Plugins.Payments.Tamara.Fields.DisplayPayLaterMessages"] = "Display Pay Later messages",
                ["Plugins.Payments.Tamara.Fields.DisplayPayLaterMessages.Hint"] = "Determine whether to display Pay Later messages. This message displays how much the customer pays in four payments. The message will be shown next to the Tamara buttons.",
            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //webhooks
            var stores = await _storeService.GetAllStoresAsync();
            var storeIds = new List<int> { 0 }.Union(stores.Select(store => store.Id));
            foreach (var storeId in storeIds)
            {
                var settings = await _settingService.LoadSettingAsync<TamaraSettings>(storeId);
                if (!string.IsNullOrEmpty(settings.WebhookUrl))
                    await _serviceManager.DeleteWebhookAsync(settings);
            }

            //settings
            if (_paymentSettings.ActivePaymentMethodSystemNames.Contains(TamaraDefault.SystemName))
            {
                _paymentSettings.ActivePaymentMethodSystemNames.Remove(TamaraDefault.SystemName);
                await _settingService.SaveSettingAsync(_paymentSettings);
            }

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(TamaraDefault.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(TamaraDefault.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.DeleteSettingAsync<TamaraSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Payments.Tamara");
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Tamara");

            await base.UninstallAsync();
        }

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }
        #endregion

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return Task.FromResult(false);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult());
        }

        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(decimal.Zero);
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //already set
            //return Task.FromResult(_actionContextAccessor.ActionContext.HttpContext.Session
            //    .Get<ProcessPaymentRequest>(TamaraDefault.PaymentRequestSessionKey));

            var session = _actionContextAccessor.ActionContext.HttpContext.Session;
            var paymentRequest = session.Get<ProcessPaymentRequest>(TamaraDefault.PaymentRequestSessionKey);
            return Task.FromResult(paymentRequest);
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payment.Tamara.PaymentMethodDescription");
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentInfoViewComponent);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return null;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {

            });
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var notConfigured = !ServiceManager.IsConfigured(_settings);
            return Task.FromResult(notConfigured);
        }

        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            return Task.CompletedTask;
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult());
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var errors = new List<string>();

            //try to get errors from the form parameters
            if (form.TryGetValue(nameof(PaymentInfoModel.Errors), out var errorValue) && !StringValues.IsNullOrEmpty(errorValue))
                errors.Add(errorValue.ToString());

            return Task.FromResult<IList<string>>(errors);
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(TamaraDefault.ConfigurationRouteName);
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            //try to get an order id from custom values
            var orderIdKey = await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.TamaraOrderId");
            var customValues = _paymentService.DeserializeCustomValues(voidPaymentRequest.Order);
            if (!customValues.TryGetValue(orderIdKey, out var orderId) || string.IsNullOrEmpty(orderId?.ToString()))
                throw new NopException("Failed to get the Tamara order ID");

            //try to get an checkout id from custom values
            var checkoutIdKey = await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.CheckoutId");
            var customCheckoutValues = _paymentService.DeserializeCustomValues(voidPaymentRequest.Order);
            if (!customValues.TryGetValue(checkoutIdKey, out var checkoutId) || string.IsNullOrEmpty(checkoutId?.ToString()))
                throw new NopException("Failed to get the Tamara order ID");


            //void previously authorized payment
            var (_, error) = await _serviceManager.VoidAsync(_settings, orderId.ToString(), checkoutId.ToString());

            if (!string.IsNullOrEmpty(error))
                return new VoidPaymentResult { Errors = new[] { error } };

            //request succeeded
            return new VoidPaymentResult { NewPaymentStatus = PaymentStatus.Voided };
        }
    }
}
