using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Tabby.Models;
using Nop.Plugin.Payments.Tabby.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Tabby.Components
{
    /// <summary>
    /// Represents the view component to display payment info in public store
    /// </summary>
    public class PaymentInfoViewComponent : NopViewComponent
    {

        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPaymentService _paymentService;
        private readonly OrderSettings _orderSettings;
        private readonly TabbySettings _settings;
        private readonly ServiceManager _serviceManager;
        private readonly IOrderProcessingService _orderProcessingService;

        #endregion

        #region Ctor

        public PaymentInfoViewComponent(ILocalizationService localizationService,
            INotificationService notificationService,
            IPaymentService paymentService,
            IOrderProcessingService orderProcessingService,
            OrderSettings orderSettings,
            TabbySettings settings,
            ServiceManager serviceManager
            )
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _paymentService = paymentService;
            _orderSettings = orderSettings;
            _settings = settings;
            _serviceManager = serviceManager;
            _orderProcessingService = orderProcessingService;
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
            var paymentRequest = new ProcessPaymentRequest();
            await _paymentService.GenerateOrderGuidAsync(paymentRequest);

            //try to create an order
            var (order, error) = await _serviceManager.CreateOrderAsync(_settings, paymentRequest.OrderGuid.ToString());



            if (order != null)
            {
                //prepare payment request GUID
                paymentRequest.CustomerId = order.CustomerId;
                paymentRequest.OrderTotal = order.OrderTotal;
                paymentRequest.StoreId = order.StoreId;
                paymentRequest.OrderGuidGeneratedOnUtc = DateTime.Now.ToUniversalTime();
                paymentRequest.PaymentMethodSystemName = TabbyDefaults.SystemName;

                model.OrderGuid = paymentRequest.OrderGuid.ToString();
                model.CheckoutUrl = order.CheckoutUrl ?? TabbyDefaults.CheckoutUrl;

                //save order details for future using
                var orderGuidKey = await _localizationService.GetResourceAsync("Plugins.Payments.Tabby.TabbyOrderId");
                paymentRequest.CustomValues.Add(orderGuidKey, order.OrderId);
            }
            else if (!string.IsNullOrEmpty(error))
            {
                model.Errors = await _localizationService.GetResourceAsync(error);
                ;
                if (_orderSettings.OnePageCheckoutEnabled)
                    ModelState.AddModelError(string.Empty, model.Errors);
                else
                    _notificationService.ErrorNotification(model.Errors);
            }

            var serializedPaymentRequest = JsonSerializer.SerializeToUtf8Bytes(paymentRequest);

            HttpContext.Session.Set(TabbyDefaults.PaymentRequestSessionKey, serializedPaymentRequest);


            return View("~/Plugins/Payments.Tabby/Views/PaymentInfo.cshtml", model);
        }
        #endregion
    }
}