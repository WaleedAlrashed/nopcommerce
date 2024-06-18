using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Tamara.Components;
using Nop.Plugin.Payments.Tamara.Domain;
using Nop.Plugin.Payments.Tamara.Models;
using Nop.Plugin.Payments.Tamara.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Payments.Tamara;

/// <summary>
/// Represents a payment method implementation
/// </summary>
public class TamaraPaymentMethod : BasePlugin, IPaymentMethod
{
    #region Fields

    protected readonly CurrencySettings _currencySettings;
    protected readonly IActionContextAccessor _actionContextAccessor;
    protected readonly ICurrencyService _currencyService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IPaymentService _paymentService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreService _storeService;
    protected readonly IUrlHelperFactory _urlHelperFactory;
    protected readonly PaymentSettings _paymentSettings;
    protected readonly TamaraSettings _settings;
    protected readonly ServiceManager _serviceManager;
    protected readonly WidgetSettings _widgetSettings;

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

    #region Methods


    /// <summary>
    /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
    /// </summary>
    /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {
        return Task.CompletedTask;
    }




    /// <summary>
    /// Process recurring payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the process payment result
    /// </returns>
    public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    /// <summary>
    /// Cancels a recurring payment
    /// </summary>
    /// <param name="cancelPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
    }

    /// <summary>
    /// Returns a value indicating whether payment method should be hidden during checkout
    /// </summary>
    /// <param name="cart">Shoping cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains true - hide; false - display.
    /// </returns>
    public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        var notConfigured = !ServiceManager.IsConfigured(_settings);
        return Task.FromResult(notConfigured);
    }

    /// <summary>
    /// Gets additional handling fee
    /// </summary>
    /// <param name="cart">Shoping cart</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the additional handling fee
    /// </returns>
    public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return Task.FromResult(decimal.Zero);
    }

    /// <summary>
    /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Validate payment form
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of validating errors
    /// </returns>
    public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var errors = new List<string>();

        //try to get errors from the form parameters
        if (form.TryGetValue(nameof(PaymentInfoModel.Errors), out var errorValue) && !StringValues.IsNullOrEmpty(errorValue))
            errors.Add(errorValue.ToString());

        return Task.FromResult<IList<string>>(errors);
    }

    /// <summary>
    /// Get payment information
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the payment info holder
    /// </returns>
    public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        //already set
        return await _actionContextAccessor.ActionContext.HttpContext.Session
            .GetAsync<ProcessPaymentRequest>(TamaraDefaults.PaymentRequestSessionKey);
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(TamaraDefaults.ConfigurationRouteName);
    }

    /// <summary>
    /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
    /// </summary>
    public Type GetPublicViewComponent()
    {
        return typeof(PaymentInfoViewComponent);
    }

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.CheckoutPaymentInfoTop,
            PublicWidgetZones.OpcContentBefore,
            PublicWidgetZones.ProductDetailsTop,
            PublicWidgetZones.ProductDetailsAddInfo,
            PublicWidgetZones.OrderSummaryContentBefore,
            PublicWidgetZones.OrderSummaryContentAfter,
            PublicWidgetZones.HeaderLinksBefore,
            PublicWidgetZones.Footer
        });
    }



    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //settings
        await _settingService.SaveSettingAsync(new TamaraSettings
        {
            PaymentType = PaymentType.Capture,

        });

        if (!_paymentSettings.ActivePaymentMethodSystemNames.Contains(TamaraDefaults.SystemName))
        {
            _paymentSettings.ActivePaymentMethodSystemNames.Add(TamaraDefaults.SystemName);
            await _settingService.SaveSettingAsync(_paymentSettings);
        }

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(TamaraDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(TamaraDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Enums.Nop.Plugin.Payments.Tamara.Domain.PaymentType.Authorize"] = "Authorize",
            ["Enums.Nop.Plugin.Payments.Tamara.Domain.PaymentType.Capture"] = "Capture",
            ["Plugins.Payments.Tamara.Configuration.Error"] = "Error: {0} (see details in the <a href=\"{1}\" target=\"_blank\">log</a>)",
            ["Plugins.Payments.Tamara.Credentials.Valid"] = "The specified credentials are valid",
            ["Plugins.Payments.Tamara.Credentials.Invalid"] = "The specified credentials are invalid",
            ["Plugins.Payments.Tamara.Fields.PaymentType"] = "Payment type",
            ["Plugins.Payments.Tamara.Fields.PaymentType.Hint"] = "Choose a payment type to either capture payment immediately or authorize a payment for an order after order creation. Notice, that alternative payment methods don't work with the 'authorize and capture later' feature.",
            ["Plugins.Payments.Tamara.Fields.SecretKey"] = "Secret",
            ["Plugins.Payments.Tamara.Fields.SecretKey.Hint"] = "Enter your PayPal REST API secret.",
            ["Plugins.Payments.Tamara.Fields.SecretKey.Required"] = "Secret is required",
            ["Plugins.Payments.Tamara.Fields.SetCredentialsManually"] = "Specify API credentials manually",
            ["Plugins.Payments.Tamara.Fields.SetCredentialsManually.Hint"] = "Determine whether to manually set the credentials (for example, there is already an app created, or if you want to use the sandbox mode).",
            ["Plugins.Payments.Tamara.Fields.UseSandbox"] = "Use sandbox",
            ["Plugins.Payments.Tamara.Fields.UseSandbox.Hint"] = "Determine whether to use the sandbox environment for testing purposes.",
            ["Plugins.Payments.Tamara.OrderId"] = "PayPal order ID",
            ["Plugins.Payments.Tamara.Prominently"] = "Feature PayPal Prominently",
            ["Plugins.Payments.Tamara.PaymentMethodDescription"] = "PayPal Checkout with using methods like Venmo, PayPal Credit, credit card payments",
            ["Plugins.Payments.Tamara.RoundingWarning"] = "It looks like you have <a href=\"{0}\" target=\"_blank\">RoundPricesDuringCalculation</a> setting disabled. Keep in mind that this can lead to a discrepancy of the order total amount, as PayPal rounds to two decimals only.",
            ["Plugins.Payments.Tamara.WebhookWarning"] = "Webhook was not created, so some functions may not work correctly (see details in the <a href=\"{0}\" target=\"_blank\">log</a>. Please ensure that your store is under SSL, PayPal service doesn't send requests to unsecured sites.)"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //webhooks
        var stores = await _storeService.GetAllStoresAsync();
        var storeIds = new List<int> { 0 }.Union(stores.Select(store => store.Id));
        foreach (var storeId in storeIds)
        {
            var settings = await _settingService.LoadSettingAsync<TamaraSettings>(storeId);

        }

        //settings
        if (_paymentSettings.ActivePaymentMethodSystemNames.Contains(TamaraDefaults.SystemName))
        {
            _paymentSettings.ActivePaymentMethodSystemNames.Remove(TamaraDefaults.SystemName);
            await _settingService.SaveSettingAsync(_paymentSettings);
        }

        if (_widgetSettings.ActiveWidgetSystemNames.Contains(TamaraDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(TamaraDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _settingService.DeleteSettingAsync<TamaraSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Payments.Tamara");
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Tamara");

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a payment method description that will be displayed on checkout pages in the public store
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.PaymentMethodDescription");
    }

    public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        throw new NotImplementedException();
    }

    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        throw new NotImplementedException();
    }

    public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        throw new NotImplementedException();
    }

    public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Properies

    /// <summary>
    /// Gets a value indicating whether capture is supported
    /// </summary>
    public bool SupportCapture => true;

    /// <summary>
    /// Gets a value indicating whether void is supported
    /// </summary>
    public bool SupportVoid => true;

    /// <summary>
    /// Gets a value indicating whether refund is supported
    /// </summary>
    public bool SupportRefund => true;

    /// <summary>
    /// Gets a value indicating whether partial refund is supported
    /// </summary>
    public bool SupportPartiallyRefund => true;

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
}