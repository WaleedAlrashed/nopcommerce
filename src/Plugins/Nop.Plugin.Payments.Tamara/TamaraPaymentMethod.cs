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
public class TamaraPaymentMethod : BasePlugin, IPaymentMethod, IWidgetPlugin
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
    /// Process a payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the process payment result
    /// </returns>
    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        //try to get an order id from custom values
        var orderIdKey = await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.OrderId");
        if (!processPaymentRequest.CustomValues.TryGetValue(orderIdKey, out var orderId) || string.IsNullOrEmpty(orderId?.ToString()))
            throw new NopException("Failed to get the PayPal order ID");

        //authorize or capture the order
        var (order, error) = _settings.PaymentType == PaymentType.Capture
            ? await _serviceManager.CaptureAsync(_settings, orderId.ToString())
            : (_settings.PaymentType == PaymentType.Authorize
                ? await _serviceManager.AuthorizeAsync(_settings, orderId.ToString())
                : (default, default));

        if (!string.IsNullOrEmpty(error))
            return new ProcessPaymentResult { Errors = new[] { error } };

        //request succeeded
        var result = new ProcessPaymentResult();

        var purchaseUnit = order.PurchaseUnits
            .FirstOrDefault(item => item.ReferenceId.Equals(processPaymentRequest.OrderGuid.ToString()));
        var authorization = purchaseUnit.Payments?.Authorizations?.FirstOrDefault();
        if (authorization != null)
        {
            result.AuthorizationTransactionId = authorization.Id;
            result.AuthorizationTransactionResult = authorization.Status;
            result.NewPaymentStatus = PaymentStatus.Authorized;
        }
        var capture = purchaseUnit.Payments?.Captures?.FirstOrDefault();
        if (capture != null)
        {
            result.CaptureTransactionId = capture.Id;
            result.CaptureTransactionResult = capture.Status;
            result.NewPaymentStatus = PaymentStatus.Paid;
        }

        return result;
    }

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
    /// Captures payment
    /// </summary>
    /// <param name="capturePaymentRequest">Capture payment request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the capture payment result
    /// </returns>
    public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        //capture a previously approved but not completed order
        if (capturePaymentRequest.Order.AuthorizationTransactionResult == "approved")
        {
            //try to get an order id from custom values
            var orderIdKey = await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.OrderId");
            var customValues = _paymentService.DeserializeCustomValues(capturePaymentRequest.Order);
            if (!customValues.TryGetValue(orderIdKey, out var orderId) || string.IsNullOrEmpty(orderId?.ToString()))
                throw new NopException("Failed to get the PayPal order ID");

            var (order, orderError) = await _serviceManager.CaptureAsync(_settings, orderId.ToString());
            if (!string.IsNullOrEmpty(orderError))
                return new CapturePaymentResult { Errors = new[] { orderError } };

            var purchaseUnit = order.PurchaseUnits
                .FirstOrDefault(item => item.ReferenceId.Equals(capturePaymentRequest.Order.OrderGuid.ToString()));
            var orderCapture = purchaseUnit.Payments?.Captures?.FirstOrDefault();
            if (orderCapture is null)
                return new CapturePaymentResult { Errors = new[] { "Order capture is empty" } };

            //request succeeded
            return new CapturePaymentResult
            {
                CaptureTransactionId = orderCapture.Id,
                CaptureTransactionResult = orderCapture.Status,
                NewPaymentStatus = PaymentStatus.Paid
            };
        }

        //or capture previously authorized payment
        var (capture, error) = await _serviceManager
            .CaptureAuthorizationAsync(_settings, capturePaymentRequest.Order.AuthorizationTransactionId);

        if (!string.IsNullOrEmpty(error))
            return new CapturePaymentResult { Errors = new[] { error } };

        //request succeeded
        return new CapturePaymentResult
        {
            CaptureTransactionId = capture.Id,
            CaptureTransactionResult = capture.Status,
            NewPaymentStatus = PaymentStatus.Paid
        };
    }

    /// <summary>
    /// Voids a payment
    /// </summary>
    /// <param name="voidPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        //void previously authorized payment
        var (_, error) = await _serviceManager.VoidAsync(_settings, voidPaymentRequest.Order.AuthorizationTransactionId);

        if (!string.IsNullOrEmpty(error))
            return new VoidPaymentResult { Errors = new[] { error } };

        //request succeeded
        return new VoidPaymentResult { NewPaymentStatus = PaymentStatus.Voided };
    }

    /// <summary>
    /// Refunds a payment
    /// </summary>
    /// <param name="refundPaymentRequest">Request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        //refund previously captured payment
        var amount = refundPaymentRequest.AmountToRefund != refundPaymentRequest.Order.OrderTotal
            ? (decimal?)refundPaymentRequest.AmountToRefund
            : null;

        //get the primary store currency
        var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)
                       ?? throw new NopException("Primary store currency cannot be loaded");

        var (refund, error) = await _serviceManager.RefundAsync(
            _settings, refundPaymentRequest.Order.CaptureTransactionId, currency.CurrencyCode, amount);

        if (!string.IsNullOrEmpty(error))
            return new RefundPaymentResult { Errors = new[] { error } };

        //request succeeded
        var refundIds = await _genericAttributeService
                            .GetAttributeAsync<List<string>>(refundPaymentRequest.Order, TamaraDefaults.RefundIdAttributeName)
                        ?? [];
        if (!refundIds.Contains(refund.Id))
            refundIds.Add(refund.Id);
        await _genericAttributeService.SaveAttributeAsync(refundPaymentRequest.Order, TamaraDefaults.RefundIdAttributeName, refundIds);
        return new RefundPaymentResult
        {
            NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
        };
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
    /// Gets a type of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component type</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        ArgumentNullException.ThrowIfNull(widgetZone);

        if (widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) ||
            widgetZone.Equals(PublicWidgetZones.OpcContentBefore) ||
            widgetZone.Equals(PublicWidgetZones.ProductDetailsTop) ||
            widgetZone.Equals(PublicWidgetZones.OrderSummaryContentBefore))
        {
            return typeof(ScriptViewComponent);
        }

        if (widgetZone.Equals(PublicWidgetZones.ProductDetailsAddInfo) || widgetZone.Equals(PublicWidgetZones.OrderSummaryContentAfter))
            return typeof(ButtonsViewComponent);

        if (widgetZone.Equals(PublicWidgetZones.HeaderLinksBefore) || widgetZone.Equals(PublicWidgetZones.Footer))
            return typeof(LogoViewComponent);

        return null;
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
            LogoInHeaderLinks = @"<!-- PayPal Logo --><li><a href=""https://www.paypal.com/webapps/mpp/paypal-popup"" title=""How PayPal Works"" onclick=""javascript:window.open('https://www.paypal.com/webapps/mpp/paypal-popup','WIPaypal','toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=yes, resizable=yes, width=1060, height=700'); return false;""><img style=""padding-top:10px;"" src=""https://www.paypalobjects.com/webstatic/mktg/logo/bdg_now_accepting_pp_2line_w.png"" border=""0"" alt=""Now accepting PayPal""></a></li><!-- PayPal Logo -->",
            LogoInFooter = @"<!-- PayPal Logo --><div><a href=""https://www.paypal.com/webapps/mpp/paypal-popup"" title=""How PayPal Works"" onclick=""javascript:window.open('https://www.paypal.com/webapps/mpp/paypal-popup','WIPaypal','toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=yes, resizable=yes, width=1060, height=700'); return false;""><img src=""https://www.paypalobjects.com/webstatic/mktg/logo/AM_mc_vs_dc_ae.jpg"" border=""0"" alt=""PayPal Acceptance Mark""></a></div><!-- PayPal Logo -->",
            StyleLayout = "vertical",
            StyleColor = "blue",
            StyleShape = "rect",
            StyleLabel = "paypal",
            DisplayButtonsOnProductDetails = true,
            DisplayButtonsOnShoppingCart = true,
            DisplayPayLaterMessages = false,
            RequestTimeout = TamaraDefaults.RequestTimeout,
            MinDiscountAmount = 0.5M
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
            ["Plugins.Payments.Tamara.Fields.ClientId"] = "Client ID",
            ["Plugins.Payments.Tamara.Fields.ClientId.Hint"] = "Enter your PayPal REST API client ID. This identifies your PayPal account and determines where transactions are paid.",
            ["Plugins.Payments.Tamara.Fields.ClientId.Required"] = "Client ID is required",
            ["Plugins.Payments.Tamara.Fields.DisplayButtonsOnProductDetails"] = "Display buttons on product details",
            ["Plugins.Payments.Tamara.Fields.DisplayButtonsOnProductDetails.Hint"] = "Determine whether to display PayPal buttons on product details pages, clicking on them matches the behavior of the default 'Add to cart' button.",
            ["Plugins.Payments.Tamara.Fields.DisplayButtonsOnShoppingCart"] = "Display buttons on shopping cart",
            ["Plugins.Payments.Tamara.Fields.DisplayButtonsOnShoppingCart.Hint"] = "Determine whether to display PayPal buttons on the shopping cart page instead of the default checkout button.",
            ["Plugins.Payments.Tamara.Fields.DisplayLogoInFooter"] = "Display logo in footer",
            ["Plugins.Payments.Tamara.Fields.DisplayLogoInFooter.Hint"] = "Determine whether to display PayPal logo in the footer. These logos and banners are a great way to let your buyers know that you choose PayPal to securely process their payments.",
            ["Plugins.Payments.Tamara.Fields.DisplayLogoInHeaderLinks"] = "Display logo in header links",
            ["Plugins.Payments.Tamara.Fields.DisplayLogoInHeaderLinks.Hint"] = "Determine whether to display PayPal logo in header links. These logos and banners are a great way to let your buyers know that you choose PayPal to securely process their payments.",
            ["Plugins.Payments.Tamara.Fields.DisplayPayLaterMessages"] = "Display Pay Later messages",
            ["Plugins.Payments.Tamara.Fields.DisplayPayLaterMessages.Hint"] = "Determine whether to display Pay Later messages. This message displays how much the customer pays in four payments. The message will be shown next to the PayPal buttons.",
            ["Plugins.Payments.Tamara.Fields.Email"] = "Email",
            ["Plugins.Payments.Tamara.Fields.Email.Hint"] = "Enter your email to get access to PayPal payments.",
            ["Plugins.Payments.Tamara.Fields.LogoInFooter"] = "Logo source code",
            ["Plugins.Payments.Tamara.Fields.LogoInFooter.Hint"] = "Enter source code of the logo. Find more logos and banners on PayPal Logo Center. You can also modify the code to fit correctly into your theme and site style.",
            ["Plugins.Payments.Tamara.Fields.LogoInHeaderLinks"] = "Logo source code",
            ["Plugins.Payments.Tamara.Fields.LogoInHeaderLinks.Hint"] = "Enter source code of the logo. Find more logos and banners on PayPal Logo Center. You can also modify the code to fit correctly into your theme and site style.",
            ["Plugins.Payments.Tamara.Fields.PaymentType"] = "Payment type",
            ["Plugins.Payments.Tamara.Fields.PaymentType.Hint"] = "Choose a payment type to either capture payment immediately or authorize a payment for an order after order creation. Notice, that alternative payment methods don't work with the 'authorize and capture later' feature.",
            ["Plugins.Payments.Tamara.Fields.SecretKey"] = "Secret",
            ["Plugins.Payments.Tamara.Fields.SecretKey.Hint"] = "Enter your PayPal REST API secret.",
            ["Plugins.Payments.Tamara.Fields.SecretKey.Required"] = "Secret is required",
            ["Plugins.Payments.Tamara.Fields.SetCredentialsManually"] = "Specify API credentials manually",
            ["Plugins.Payments.Tamara.Fields.SetCredentialsManually.Hint"] = "Determine whether to manually set the credentials (for example, there is already an app created, or if you want to use the sandbox mode).",
            ["Plugins.Payments.Tamara.Fields.UseSandbox"] = "Use sandbox",
            ["Plugins.Payments.Tamara.Fields.UseSandbox.Hint"] = "Determine whether to use the sandbox environment for testing purposes.",
            ["Plugins.Payments.Tamara.Onboarding.AccessRevoked"] = "Profile access has been successfully revoked.",
            ["Plugins.Payments.Tamara.Onboarding.Button"] = "Sign up for PayPal",
            ["Plugins.Payments.Tamara.Onboarding.ButtonRevoke"] = "Revoke access",
            ["Plugins.Payments.Tamara.Onboarding.Completed"] = "Onboarding is sucessfully completed",
            ["Plugins.Payments.Tamara.Onboarding.EmailSet"] = "Email is set, now you are ready to sign up for PayPal",
            ["Plugins.Payments.Tamara.Onboarding.Error"] = "An error occurred during the onboarding process, the credentials are empty",
            ["Plugins.Payments.Tamara.Onboarding.InProcess"] = "Onboarding is in process, see details below",
            ["Plugins.Payments.Tamara.Onboarding.Process.Account"] = "PayPal account is created",
            ["Plugins.Payments.Tamara.Onboarding.Process.Email"] = "Email address is confirmed",
            ["Plugins.Payments.Tamara.Onboarding.Process.Payments"] = "Billing information is set",
            ["Plugins.Payments.Tamara.Onboarding.Process.Permission"] = "Permissions are granted",
            ["Plugins.Payments.Tamara.Onboarding.Title"] = "Sign up for PayPal",
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
            if (!string.IsNullOrEmpty(settings.WebhookUrl))
                await _serviceManager.DeleteWebhookAsync(settings);
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
    /// Update plugin
    /// </summary>
    /// <param name="currentVersion">Current version of plugin</param>
    /// <param name="targetVersion">New version of plugin</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        var current = decimal.TryParse(currentVersion, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 1.00M;

        //new setting added in 1.09
        if (current < 1.09M)
        {
            var settings = await _settingService.LoadSettingAsync<TamaraSettings>();
            if (!await _settingService.SettingExistsAsync(settings, setting => setting.MinDiscountAmount))
            {
                settings.MinDiscountAmount = 0.5M;
                await _settingService.SaveSettingAsync(settings);
            }
        }
    }

    /// <summary>
    /// Gets a payment method description that will be displayed on checkout pages in the public store
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.PaymentMethodDescription");
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