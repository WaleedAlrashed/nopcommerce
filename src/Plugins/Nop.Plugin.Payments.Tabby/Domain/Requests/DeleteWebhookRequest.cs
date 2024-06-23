﻿using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Nop.Plugin.Payments.Tabby.Domain.Requests
{
    public class DeleteWebhookRequest: Request
    {
        public DeleteWebhookRequest(string webhookId)
        {
            WebhookId = webhookId;
        }
        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public string WebhookId { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Path => $"/webhooks/{WebhookId}";

        /// <summary>
        /// Gets the request path
        /// </summary>
        [JsonIgnore]
        public override string Method => HttpMethods.Delete;
    }
}
