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
    /// Represents a payment method implementation
    /// </summary>
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

        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return await Task.FromResult(new CancelRecurringPaymentResult());
        }

        public async Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            return await Task.FromResult(false);
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return await Task.FromResult(new CapturePaymentResult());
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await Task.FromResult(0m);
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return await Task.FromResult(new ProcessPaymentRequest());
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await Task.FromResult("Tabby Payment Method");
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentInfoViewComponent);
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return await Task.FromResult(false);
        }


        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return await Task.FromResult(new ProcessPaymentResult());
        }

        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return await Task.FromResult(new ProcessPaymentResult());
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return await Task.FromResult(new RefundPaymentResult());
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return await Task.FromResult(new List<string>());
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return await Task.FromResult(new VoidPaymentResult());
        }

        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(TabbyDefaults.ConfigurationRouteName);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;


            var url = "https://api.tabby.ai/api/v2/checkout";


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
                ""reference_id"": ""{order.Id}"", 
                ""items"": [
                    {{
                        ""title"": ""Product Title"", 
                        ""description"": ""Product Description"", 
                        ""quantity"": 1, 
                        ""unit_price"": ""100"", 
                        ""discount_amount"": ""0.00"",
                        ""reference_id"": ""Product Ref ID"",
                        ""image_url"": ""http://example.com"",
                        ""product_url"": ""http://example.com"",
                        ""gender"": ""Male"",
                        ""category"": ""Category"",  
                        ""color"": ""Color"",
                        ""product_material"": ""Material"",
                        ""size_type"": ""Size Type"",
                        ""size"": ""Size"",
                        ""brand"": ""Brand""
                    }}
                ]
            }},
            ""meta"": {{
                ""order_id"": ""{order.CustomOrderNumber}"", 
                ""customer"": ""{order.CustomerId}""
            }}
        }},
        ""lang"": ""en"", 
        ""merchant_code"": ""KSECRETUAE"", 
        ""merchant_urls"": {{
            ""success"": ""https://localhost:44369/"",
            ""cancel"": ""https://localhost:44369/"",
            ""failure"": ""https://localhost:44369/""
        }}
    }}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer pk_test_80d3109b-b620-4121-bb99-02cb63faef76");
                client.DefaultRequestHeaders.Add("Cookie", "_cfuvid=ontr9nE4bG5oNeoCtSQDAGby0ZAeJyA4jfHmVfywTjs-1718291297463-0.0.1.1-604800000");

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
