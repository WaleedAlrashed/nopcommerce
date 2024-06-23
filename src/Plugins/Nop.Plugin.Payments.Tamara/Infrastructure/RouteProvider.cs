﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Payments.Tamara.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(TamaraDefault.ConfigurationRouteName,
                "Admin/Tamara/Configure",
                new { controller = "Tamara", action = "Configure" });

            endpointRouteBuilder.MapControllerRoute(TamaraDefault.WebhookRouteName,
                "Plugins/TamaraWebhook/Notification",
                new { controller = "TamaraWebhook", action = "WebhookHandler" });
        }
        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}