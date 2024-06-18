﻿using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Tamara.Models;
using Nop.Plugin.Payments.Tamara.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Tamara.Components;

/// <summary>
/// Represents the view component to display payment info in public store
/// </summary>
public class PaymentInfoViewComponent : NopViewComponent
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPaymentService _paymentService;
    protected readonly OrderSettings _orderSettings;
    protected readonly TamaraSettings _settings;
    protected readonly ServiceManager _serviceManager;

    #endregion

    #region Ctor

    public PaymentInfoViewComponent(ILocalizationService localizationService,
        INotificationService notificationService,
        IPaymentService paymentService,
        OrderSettings orderSettings,
        TamaraSettings settings,
        ServiceManager serviceManager)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _paymentService = paymentService;
        _orderSettings = orderSettings;
        _settings = settings;
        _serviceManager = serviceManager;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invoke view component
    /// </summary>
    /// <param name="widgetZone">Widget zone name</param>
    /// <param name="additionalData">Additional data</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var model = new PaymentInfoModel();

        //prepare order GUID
        var paymentRequest = new ProcessPaymentRequest();
        await _paymentService.GenerateOrderGuidAsync(paymentRequest);

        //try to create an order

        await HttpContext.Session.SetAsync(TamaraDefaults.PaymentRequestSessionKey, paymentRequest);

        return View("~/Plugins/Payments.Tamara/Views/PaymentInfo.cshtml", model);
    }

    #endregion
}