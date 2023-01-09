// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Shared;
using System.Net.Http;
using System.Net;

namespace Pomelo.DevOps.Agent.Controllers
{
    [Route("api/project/{projectId}/pipeline/{pipelineId}/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private static readonly HttpClient client = HttpClientFactory.CreateHttpClient(Program.IgnoreSslErrors);

        [HttpGet("{*endpoint}")]
        public async ValueTask<IActionResult> GetProxy(
            [FromServices] ConfigManager config,
            [FromRoute] string projectId,
            [FromRoute] string pipelineId,
            [FromRoute] long jobNumber,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            if (!IPAddress.IsLoopback(HttpContext.Connection.RemoteIpAddress))
            {
                return BadRequest("Invalid Client");
            }

            var serverEntryPoint = $"{config.Config.Server.TrimEnd('/')}/api/project/{projectId}/pipeline/{pipelineId}/job/{endpoint}";
            var request = Request.ToHttpRequestMessage(serverEntryPoint, new Dictionary<string, string> 
            {
                ["Authorization"] = $"Token {config.Config.PersonalAccessToken}"
            });
            var response = await client.SendAsync(request, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response, cancellationToken);
            return new EmptyResult();
        }

        [HttpPost("{*endpoint}")]
        public async ValueTask<IActionResult> PostProxy(
            [FromServices] ConfigManager config,
            [FromRoute] string projectId,
            [FromRoute] string pipelineId,
            [FromRoute] long jobNumber,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            if (!IPAddress.IsLoopback(HttpContext.Connection.RemoteIpAddress))
            {
                return BadRequest("Invalid Client");
            }

            var serverEntryPoint = $"{config.Config.Server.TrimEnd('/')}/api/project/{projectId}/pipeline/{pipelineId}/job/{endpoint}";
            var request = Request.ToHttpRequestMessage(serverEntryPoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {config.Config.PersonalAccessToken}"
            });
            var response = await client.SendAsync(request, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response, cancellationToken);
            return new EmptyResult();
        }

        [HttpPatch("{*endpoint}")]
        public async ValueTask<IActionResult> PatchProxy(
            [FromServices] ConfigManager config,
            [FromRoute] string projectId,
            [FromRoute] string pipelineId,
            [FromRoute] long jobNumber,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            if (!IPAddress.IsLoopback(HttpContext.Connection.RemoteIpAddress))
            {
                return BadRequest("Invalid Client");
            }

            var serverEntryPoint = $"{config.Config.Server.TrimEnd('/')}/api/project/{projectId}/pipeline/{pipelineId}/job/{endpoint}";
            var request = Request.ToHttpRequestMessage(serverEntryPoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {config.Config.PersonalAccessToken}"
            });
            var response = await client.SendAsync(request, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response, cancellationToken);
            return new EmptyResult();
        }

        [HttpPut("{*endpoint}")]
        public async ValueTask<IActionResult> PutProxy(
            [FromServices] ConfigManager config,
            [FromRoute] string projectId,
            [FromRoute] string pipelineId,
            [FromRoute] long jobNumber,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            if (!IPAddress.IsLoopback(HttpContext.Connection.RemoteIpAddress))
            {
                return BadRequest("Invalid Client");
            }

            var serverEntryPoint = $"{config.Config.Server.TrimEnd('/')}/api/project/{projectId}/pipeline/{pipelineId}/job/{endpoint}";
            var request = Request.ToHttpRequestMessage(serverEntryPoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {config.Config.PersonalAccessToken}"
            });
            var response = await client.SendAsync(request, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response, cancellationToken);
            return new EmptyResult();
        }

        [HttpDelete("{*endpoint}")]
        public async ValueTask<IActionResult> DeleteProxy(
            [FromServices] ConfigManager config,
            [FromRoute] string projectId,
            [FromRoute] string pipelineId,
            [FromRoute] long jobNumber,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            if (!IPAddress.IsLoopback(HttpContext.Connection.RemoteIpAddress))
            {
                return BadRequest("Invalid Client");
            }

            var serverEntryPoint = $"{config.Config.Server.TrimEnd('/')}/api/project/{projectId}/pipeline/{pipelineId}/job/{endpoint}";
            var request = Request.ToHttpRequestMessage(serverEntryPoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {config.Config.PersonalAccessToken}"
            });
            var response = await client.SendAsync(request, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response, cancellationToken);
            return new EmptyResult();
        }
    }
}
