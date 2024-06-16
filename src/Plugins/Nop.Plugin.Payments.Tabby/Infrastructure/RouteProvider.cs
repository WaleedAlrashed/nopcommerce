using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Tabby.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: TabbyDefaults.ConfigurationRouteName,
            pattern: "Admin/Tabby/Configure",
            defaults: new { controller = "Tabby", action = "Configure", area = AreaNames.ADMIN }
            );

        endpointRouteBuilder.MapControllerRoute(TabbyDefaults.WebhookRouteName,
            "Plugins/Tabby/Webhook",
            new { controller = "TabbyWebhook", action = "WebhookHandler" });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}