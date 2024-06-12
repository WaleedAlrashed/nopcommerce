using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Tabby.Components
{
    /// <summary>
    /// Represents the view component to display payment info in public store
    /// </summary>
    public class PaymentInfoViewComponent : NopViewComponent
    {
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

            return View("~/Plugins/Payments.Tabby/Views/PaymentInfo.cshtml");
        }
        #endregion
    }
}