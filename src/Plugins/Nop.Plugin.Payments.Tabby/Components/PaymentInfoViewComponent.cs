using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Tabby.Domain.Requests;
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
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaymentInfoViewComponent(ILocalizationService localizationService,
            INotificationService notificationService,
            IPaymentService paymentService,
            IOrderProcessingService orderProcessingService,
            OrderSettings orderSettings,
            TabbySettings settings,
            ServiceManager serviceManager,
            IWebHelper webHelper,
            IStoreContext storeContext
            )
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _paymentService = paymentService;
            _orderSettings = orderSettings;
            _settings = settings;
            _serviceManager = serviceManager;
            _orderProcessingService = orderProcessingService;
            _webHelper = webHelper;
            _storeContext = storeContext;
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


            var store = await _storeContext.GetCurrentStoreAsync();
            var storeId = store.Id;




            ////try to create an order
            var (checkoutRequest, error) = await _serviceManager.CreateTabbyCheckoutRequestAsync(_settings, paymentRequest.OrderGuid.ToString());

            var jsonContent = JsonConvert.SerializeObject(checkoutRequest);

            var redirectUrl = "";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.PublicKey}");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(TabbyDefaults.CheckoutUrl, content);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var rootObject = JsonConvert.DeserializeObject<RootObject>(result);
                    redirectUrl = rootObject.configuration.available_products.installments[0].web_url;

                    // Redirect to the Tabby payment page

                    //var httpContext = EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext;
                    //httpContext.Response.Redirect(redirectUrl);
                }
                else
                {
                    _notificationService.ErrorNotification("Failed To create tabby payment");
                    //throw new Exception("Failed to create Tabby payment");
                }


                if (redirectUrl != null && checkoutRequest != null)
                {
                    //var checkoutRequestParsed = JObject.Parse(checkoutRequest);
                    //prepare payment request GUID
                    //paymentRequest.CustomerId = int.Parse((string)checkoutRequestParsed["payment"]["meta"]["customer_id"]);
                    //paymentRequest.OrderTotal = (decimal)checkoutRequestParsed["payment"]["amount"];
                    paymentRequest.StoreId = storeId;
                    paymentRequest.OrderGuidGeneratedOnUtc = DateTime.Now.ToUniversalTime();
                    paymentRequest.PaymentMethodSystemName = TabbyDefaults.SystemName;

                    model.OrderGuid = paymentRequest.OrderGuid.ToString();
                    model.CheckoutUrl = redirectUrl;

                    //save order details for future using
                    var orderGuidKey = await _localizationService.GetResourceAsync("Plugins.Payments.Tabby.TabbyOrderId") ?? "order_guid";
                    paymentRequest.CustomValues.Add(orderGuidKey, paymentRequest.OrderGuid.ToString());
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    model.Errors = await _localizationService.GetResourceAsync(error);

                    if (_orderSettings.OnePageCheckoutEnabled)
                    {
                        ModelState.AddModelError(string.Empty, model.Errors);
                    }
                    else
                    {
                        _notificationService.ErrorNotification(model.Errors);
                    }
                }

                var serializedPaymentRequest = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(paymentRequest);

                HttpContext.Session.Set(TabbyDefaults.PaymentRequestSessionKey, serializedPaymentRequest);


                return View("~/Plugins/Payments.Tabby/Views/PaymentInfo.cshtml", model);
            }
            #endregion
        }
    }
}