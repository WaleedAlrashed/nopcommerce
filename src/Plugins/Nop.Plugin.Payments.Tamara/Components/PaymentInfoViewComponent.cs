using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Tamara.Models;
using Nop.Plugin.Payments.Tamara.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Tamara.Components
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
        private readonly TamaraSettings _settings;
        private readonly ServiceManager _serviceManager;
        private readonly IOrderProcessingService _orderProcessingService;

        #endregion

        #region Ctor

        public PaymentInfoViewComponent(ILocalizationService localizationService,
            INotificationService notificationService,
            IPaymentService paymentService,
            IOrderProcessingService orderProcessingService,
            OrderSettings orderSettings,
            TamaraSettings settings,
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
             _paymentService.GenerateOrderGuid(paymentRequest);

            //try to create an order
            var (order, error) = await _serviceManager.CreateOrderAsync(_settings, paymentRequest.OrderGuid.ToString());



            if (order != null)
            {
                //prepare payment request GUID
                paymentRequest.CustomerId = order.CustomerId;
                paymentRequest.OrderTotal = order.OrderTotal;
                paymentRequest.StoreId = order.StoreId;
                paymentRequest.OrderGuidGeneratedOnUtc = DateTime.Now.ToUniversalTime();
                paymentRequest.PaymentMethodSystemName = TamaraDefault.SystemName;

                model.OrderGuid = paymentRequest.OrderGuid.ToString();
                model.CheckoutUrl = order.CheckoutUrl;

                //save order details for future using
                var orderGuidKey = await _localizationService.GetResourceAsync("Plugins.Payments.Tamara.TamaraOrderId");
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

            HttpContext.Session.Set(TamaraDefault.PaymentRequestSessionKey, serializedPaymentRequest);

            return View("~/Plugins/Payments.Tamara/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}