// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pomelo.DevOps.Triggers.Scheduled.Models.ViewModels;
using System.Reflection;

namespace Pomelo.DevOps.Triggers.Scheduled.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        [HttpGet]
        public async ValueTask<ActionResult<ServerConfig>> Get(
            CancellationToken cancellationToken = default)
        {
            var serverConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "server.json");
            var serverConfig = JsonConvert.DeserializeObject<ServerConfig>(await System.IO.File.ReadAllTextAsync(serverConfigPath, cancellationToken));
            return Ok(serverConfig);
        }

        [HttpPut]
        [HttpPost]
        [HttpPatch]
        public async ValueTask<ActionResult<ServerConfig>> Patch(
            [FromBody] ServerConfig config,
            CancellationToken cancellationToken = default)
        {
            var serverConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "server.json");
            await System.IO.File.WriteAllTextAsync(serverConfigPath, JsonConvert.SerializeObject(config), cancellationToken);
            return await Get(cancellationToken);
        }
    }
}
