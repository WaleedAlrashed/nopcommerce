using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tamara.Domain.Requests
{
    internal class OrderDetailsRequest : Request
    {
        public OrderDetailsRequest(string tamaraOrderId)
        {
            TamaraOrderId = tamaraOrderId;
        }
        /// <summary>
        /// Gets the order id
        /// </summary>
        [JsonIgnore]
        public string TamaraOrderId { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Path => $"/orders/{TamaraOrderId}";

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Method => HttpMethods.Get;
    }
}