using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Plugin.Payments.Tamara.Domain.RequestParams;

namespace Nop.Plugin.Payments.Tamara.Domain.Requests
{
    public class VoidRequest: Request
    {
        public VoidRequest(string orderId, string checkoutId)
        {
            CheckoutId = checkoutId;
            OrderId = orderId;
        }

        /// <summary>
        /// Gets or sets order id
        /// </summary>
        [JsonIgnore]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets order id
        /// </summary>
        [JsonIgnore]
        public string CheckoutId { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Path => $"checkout/{OrderId}/void?order_id={OrderId}";

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Method => HttpMethods.Post;
    }
}
