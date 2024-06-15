using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Tabby.Components;
using Nop.Plugin.Payments.Tabby.Models;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Tabby
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TabbyPaymentMethod"/> class.
    /// </summary>
    /// <param name="webHelper">The web helper.</param>
    /// <param name="urlHelper">The URL helper factory.</param>
    /// <param name="actionContextAccessor">The action context accessor.</param>
    public class TabbyPaymentMethod : BasePlugin, IPaymentMethod
    {
        public TabbyPaymentMethod(IWebHelper webHelper,
             IUrlHelperFactory urlHelper,
             IActionContextAccessor actionContextAccessor

            )
        {
            _webHelper = webHelper;
            _urlHelperFactory = urlHelper;
            _actionContextAccessor = actionContextAccessor;
        }

        protected readonly IActionContextAccessor _actionContextAccessor;
        protected readonly IWebHelper _webHelper;
        protected readonly IUrlHelperFactory _urlHelperFactory;


        /// <summary>
        /// Gets a value indicating whether capture is supported.
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported.
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported.
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether void is supported.
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets the recurring payment type.
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets the payment method type.
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether payment info is skipped.
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Cancels a recurring payment.
        /// </summary>
        /// <param name="cancelPaymentRequest">The cancel payment request.</param>
        /// <returns>The cancel recurring payment result.</returns>
        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return await Task.FromResult(new CancelRecurringPaymentResult());
        }

        /// <summary>
        /// Determines whether payment can be reposted.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns>A boolean value indicating whether the payment can be reposted.</returns>
        public async Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Captures a payment.
        /// </summary>
        /// <param name="capturePaymentRequest">The capture payment request.</param>
        /// <returns>The capture payment result.</returns>
        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return await Task.FromResult(new CapturePaymentResult());
        }

        /// <summary>
        /// Gets the additional handling fee.
        /// </summary>
        /// <param name="cart">The shopping cart items.</param>
        /// <returns>The additional handling fee.</returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await Task.FromResult(0m);
        }

        /// <summary>
        /// Gets the payment info.
        /// </summary>
        /// <param name="form">The form collection.</param>
        /// <returns>The process payment request.</returns>
        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return await Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets the payment method description.
        /// </summary>
        /// <returns>The payment method description.</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await Task.FromResult("Tabby Payment Method");
        }

        /// <summary>
        /// Gets the public view component.
        /// </summary>
        /// <returns>The type of the public view component.</returns>
        public Type GetPublicViewComponent()
        {
            return typeof(PaymentInfoViewComponent);
        }

        /// <summary>
        /// Determines whether the payment method should be hidden.
        /// </summary>
        /// <param name="cart">The shopping cart items.</param>
        /// <returns>A boolean value indicating whether the payment method should be hidden.</returns>
        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="processPaymentRequest">The process payment request.</param>
        /// <returns>The process payment result.</returns>
        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return await Task.FromResult(new ProcessPaymentResult());
        }

        /// <summary>
        /// Processes the recurring payment.
        /// </summary>
        /// <param name="processPaymentRequest">The process payment request.</param>
        /// <returns>The process payment result.</returns>
        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return await Task.FromResult(new ProcessPaymentResult());
        }

        /// <summary>
        /// Refunds the payment.
        /// </summary>
        /// <param name="refundPaymentRequest">The refund payment request.</param>
        /// <returns>The refund payment result.</returns>
        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await Task.FromResult(new RefundPaymentResult());
        }

        /// <summary>
        /// Validates the payment form.
        /// </summary>
        /// <param name="form">The form collection.</param>
        /// <returns>A list of validation errors.</returns>
        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return await Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Voids the payment.
        /// </summary>
        /// <param name="voidPaymentRequest">The void payment request.</param>
        /// <returns>The void payment result.</returns>
        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return await Task.FromResult(new VoidPaymentResult());
        }

        /// <summary>
        /// Gets the configuration page URL.
        /// </summary>
        /// <returns>The configuration page URL.</returns>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(TabbyDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Post-processes the payment.
        /// </summary>
        /// <param name="postProcessPaymentRequest">The post-process payment request.</param>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;

            var url = TabbyDefaults.CheckoutUrl;

            var jsonContent = $@"{{
        ""payment"": {{
            ""amount"": ""{order.OrderTotal}"", 
            ""currency"": ""AED"", 
            ""description"": ""Order # {order.Id}"",
            ""buyer"": {{
                ""phone"": ""500000001"", 
                ""email"": ""card.success@tabby.ai"", 
                ""name"": ""string"",
                ""dob"": ""2019-08-24""
            }},
            ""buyer_history"": {{
                ""registered_since"": ""{order.CreatedOnUtc:yyyy-MM-ddTHH:mm:ssZ}"", 
                ""loyalty_level"": 0,
                ""wishlist_count"": 0, 
                ""is_social_networks_connected"": true,
                ""is_phone_number_verified"": true, 
                ""is_email_verified"": true 
            }},
            ""order"": {{
                ""tax_amount"": ""{order.OrderTax}"",
                ""shipping_amount"": ""{order.OrderShippingInclTax}"",
                ""discount_amount"": ""{order.OrderDiscount}"",
                ""updated_at"": ""{order.CreatedOnUtc:yyyy-MM-ddTHH:mm:ssZ}"",
                ""reference_id"": ""{order.Id}""     
            }},
            ""meta"": {{
                ""order_id"": ""{order.CustomOrderNumber}"", 
                ""customer"": ""{order.CustomerId}""
            }}
        }},
        ""lang"": ""en"", 
        ""merchant_code"": ""KSECRETUAE"", 
        ""merchant_urls"": {{
            ""success"": ""{_webHelper.GetStoreHost(false)}"",
            ""cancel"": ""{_webHelper.GetStoreHost(false)}"",
            ""failure"": ""{_webHelper.GetStoreHost(false)}""
        }}
    }}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer pk_test_80d3109b-b620-4121-bb99-02cb63faef76");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var rootObject = JsonConvert.DeserializeObject<RootObject>(result);
                    var redirectUrl = rootObject.configuration.available_products.installments[0].web_url;

                    // Redirect to the Tabby payment page

                    var httpContext = EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext;
                    httpContext.Response.Redirect(redirectUrl);
                }
                else
                {
                    throw new Exception("Failed to create Tabby payment");
                }

            }
        }

    }
}
