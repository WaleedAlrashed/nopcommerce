using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tamara.Domain.Requests
{
    public class AuthoriseRequest: Request
    {
        public AuthoriseRequest(string orderId)
        {
            this.OrderId = orderId;
        }
        /// <summary>
        /// Gets or sets order id
        /// </summary>
        [JsonIgnore]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Path => $"orders/{OrderId}/authorise";

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Method => HttpMethods.Post;
    }
}
