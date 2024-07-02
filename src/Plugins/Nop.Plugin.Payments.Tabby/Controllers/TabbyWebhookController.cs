using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Tabby.Services;

namespace Nop.Plugin.Payments.Tabby.Controllers
{
    public class TabbyWebhookController : Controller
    {
        #region Fields

        private readonly TabbySettings _settings;
        private readonly ServiceManager _serviceManager;

        #endregion

        #region Ctor

        public TabbyWebhookController(TabbySettings settings,
            ServiceManager serviceManager)
        {
            _settings = settings;
            _serviceManager = serviceManager;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<IActionResult> WebhookHandler()
        {
            var baseUri = $"{Request.Scheme}://{Request.Host}";

            if (string.IsNullOrEmpty(Request?.QueryString.Value))
                return Redirect($"{baseUri}/cart");
            
            var requestParams = GetParamsAsync(queryString: Request?.QueryString.Value);
            var status = requestParams["?paymentStatus"];
            var tabbyOrderId = requestParams["orderId"];

            if (string.IsNullOrEmpty(status) || string.IsNullOrEmpty(tabbyOrderId))
                return Redirect($"{baseUri}/cart");
                
            if (status != "approved")
                return Redirect($"{baseUri}/cart");
                
            await _serviceManager.HandleWebhookAsync(_settings, tabbyOrderId);
                
            return Redirect($"{baseUri}/checkout/completed");

        }

        private Dictionary<string, string> GetParamsAsync(string queryString)
        {
            var dic = new Dictionary<string, string>();

            var paramsList = queryString.Split('&');
            foreach (var param in paramsList)
            {
                var keyAndValue = param.Split('=');
                if (keyAndValue.Length == 2)
                {
                    dic.Add(keyAndValue[0].Trim(), keyAndValue[1].Trim());
                }
            }

            return dic;
        }

        #endregion
    }
}
