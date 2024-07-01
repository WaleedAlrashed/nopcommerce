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

            if (!string.IsNullOrEmpty(Request?.QueryString.Value))
            {
                var requestParams = GetParamsAsync(queryString: Request?.QueryString.Value);
                var status = requestParams["?paymentStatus"];
                var tamaraOrderId = requestParams["orderId"];

                if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(tamaraOrderId))
                {
                    if (status == "approved")
                    {
                        await _serviceManager.HandleWebhookAsync(_settings, tamaraOrderId);
                        return Redirect($"{baseUri}/checkout/completed");
                    }
                }
            }

            return Redirect($"{baseUri}/cart");
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
