// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.DevOps.Agent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        [HttpPost]
        public async ValueTask<ApiResult> Post(
            [FromServices] Connector connector,
            [FromServices] ConfigManager _config,
            [FromServices] StepManager _gallery,
            Config config,
            CancellationToken cancellationToken = default)
        {
            _config.Config.AgentId = default;
            _config.Config.AgentPoolId = config.AgentPoolId;
            _config.Config.Identifier = config.Identifier;
            _config.Config.ProjectId = config.ProjectId;
            _config.Config.PersonalAccessToken = config.PersonalAccessToken;
            _config.Config.Server = config.Server;
            _config.Config.RemoteControl = config.RemoteControl;
            if (_config.Config.GalleryFeeds == null)
            {
                _config.Config.GalleryFeeds = new List<string> { _config.Config.Server };
            }
            if (_config.Config.GalleryFeeds.All(x => x != _config.Config.Server)) 
            {
                _config.Config.GalleryFeeds.Add(_config.Config.Server);
            }
            _config.Save();
            connector.RefreshHeaders();
            _gallery.ClearFeed();
            foreach(var x in _config.Config.GalleryFeeds)
            {
                _gallery.AddFeed(x);
            }
            await connector.RegisterAgentAsync(cancellationToken);
            return ApiResult(200, "Agent configuration updated");
        }
    }
}
