// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using Pomelo.DevOps.Agent;

namespace Pomelo.DevOps.Agent.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        public IActionResult Index(
            [FromServices] ConfigManager configManager)
        {
            if (configManager.Config.AgentId == null)
            {
                return RedirectToAction("Install");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Install() => View();

        [HttpPost]
        public async ValueTask<IActionResult> Install(
            [FromServices] Connector connector,
            [FromServices] ConfigManager _config,
            [FromServices] ConfigController configController,
            [FromServices] StepManager gallery,
            Config config,
            CancellationToken cancellationToken = default)
        {
            await configController.Post(connector, _config, gallery, config, cancellationToken);
            return RedirectToAction("Index");
        }

        public IActionResult Reboot()
        {
            Process.Start("powershell", "-c Restart-Computer -Force");
            return Content("OK");
        }

        public IActionResult Status([FromServices] StageContainer stageContainer)
        {
            return Json(stageContainer.Stages);
        }
    }
}
