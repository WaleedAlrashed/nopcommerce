using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Tamara.Services;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Tamara.Controllers
{
    public class TamaraCheckoutController : BasePluginController
    {
        #region Fields

        private readonly TamaraSettings _settings;
        private readonly ServiceManager _serviceManager;

        #endregion

        #region Ctor

        public TamaraCheckoutController(TamaraSettings settings,
            ServiceManager serviceManager)
        {
            _settings = settings;
            _serviceManager = serviceManager;
        }

        #endregion

        #region Method

        [HttpPost]
        public async Task<IActionResult> Checkout(string id)
        {
            var result = await _serviceManager.HandleCheckoutRequestAsync(_settings, id);
            return Json(new { Data = result });
        }

        #endregion
    }
}
