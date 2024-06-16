using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Tamara.Services;

namespace Nop.Plugin.Payments.Tamara.Controllers;

public class TamaraWebhookController : Controller
{
    #region Fields

    protected readonly TamaraSettings _settings;
    protected readonly ServiceManager _serviceManager;

    #endregion

    #region Ctor

    public TamaraWebhookController(TamaraSettings settings,
        ServiceManager serviceManager)
    {
        _settings = settings;
        _serviceManager = serviceManager;
    }

    #endregion

    #region Methods

    [HttpPost]
    public async Task<IActionResult> WebhookHandler()
    {
        await _serviceManager.HandleWebhookAsync(_settings, Request);
        return Ok();
    }

    #endregion
}