using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Tabby.Components
{
    public class PaymentInfoViewComponent : NopViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            // Add any logic you need to pass data to the view here

            return View("~/Plugins/Payments.Tabby/Views/PaymentInfo.cshtml");
        }
    }
}