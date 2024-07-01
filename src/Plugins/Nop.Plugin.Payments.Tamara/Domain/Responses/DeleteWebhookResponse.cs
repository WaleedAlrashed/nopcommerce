using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Tamara.Domain.Responses
{
    public class DeleteWebhookResponse: Response
    {
        /// <summary>
        /// Gets or sets message
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
