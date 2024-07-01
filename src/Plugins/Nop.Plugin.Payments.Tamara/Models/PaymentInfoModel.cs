using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Tamara.Models
{
    /// <summary>
    /// Represents a payment info model
    /// </summary>
    public record PaymentInfoModel : BaseNopModel
    {
        #region Properties

        public string OrderGuid { get; set; }
        public string CheckoutUrl { get; set; }

        public string Errors { get; set; }

        #endregion
    }
}