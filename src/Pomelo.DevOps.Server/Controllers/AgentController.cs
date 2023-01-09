// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Server.MetricsManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/project/{projectId}/agentpool/{pool}/[controller]")]
    [ApiController]
    [Authorize]
    public class AgentController : ControllerBase
    {
        /// <summary>
        /// Get agents from an agent pool
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pool">Pool ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async ValueTask<ApiResult<List<Agent>>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] long pool,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return ApiResult<List<Agent>>(401, "You don't have the permission to this project");
            }

            var _agents = await db.Agents
                .Where(x => x.AgentPoolId == pool)
                .ToListAsync(cancellationToken);
            var agents = new ConcurrentBag<Agent>();
            foreach (var x in _agents)
            {
                agents.Add(x);
            }

            var ips = agents.Select(x => x.Address);
            await Task.WhenAll(ips.Select(x => Task.Run(async () =>
            {
                try
                {
                    using var client = new HttpClient() { BaseAddress = new Uri($"http://{x}:5500"), Timeout = new TimeSpan(0, 0, 3) };
                    using var response = await client.GetAsync("/home/status");

                    var str = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<IEnumerable<JobStage>>(str);
                    if (result.Count() == 0)
                    {
                        foreach (var agent in agents.Where(y => y.Address == x))
                        {
                            agent.Status = AgentStatus.Idle;
                        }
                    }
                    else
                    {
                        foreach (var agent in agents.Where(y => y.Address == x))
                        {
                            agent.Status = AgentStatus.Busy;
                        }
                    }

                }
                catch
                {
                    foreach (var agent in agents.Where(y => y.Address == x))
                    {
                        agent.Status = AgentStatus.Offline;
                    }
                }
            })));

            return ApiResult(agents.ToList());
        }

        [HttpGet("{agent}")]
        public async ValueTask<ApiResult<Agent>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] long pool,
            [FromRoute] Guid agent,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return ApiResult<Agent>(401, "You don't have the permission to this project");
            }

            var _agent = await db.Agents
                .Where(x => x.AgentPoolId == pool)
                .Where(x => x.Id == agent)
                .SingleOrDefaultAsync(cancellationToken);

            if (_agent == null)
            {
                return ApiResult<Agent>(404, "The agent has not been found");
            }

            return ApiResult(_agent);
        }

        [HttpGet("{agent}/metrics")]
        public async ValueTask<ApiResult<IEnumerable<Metrics>>> GetMetrics(
            [FromServices] PipelineContext db,
            [FromServices] IMetricsManager metricsManager,
            [FromRoute] string projectId,
            [FromRoute] Guid agent,
            [FromQuery] MetricsType type,
            [FromQuery] DateTimeOffset begin,
            [FromQuery] DateTimeOffset? end = null,
            CancellationToken cancellationToken = default)
        {
            if (end == null)
            {
                end = DateTimeOffset.UtcNow;
            }

            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return ApiResult<IEnumerable<Metrics>>(401, "You don't have the permission to this project");
            }

            var result = await metricsManager.GetAsync(agent, type, begin.Ticks, end.Value.Ticks, cancellationToken);
            return ApiResult(result);
        }

        [HttpPost("{agent}/metrics")]
        public async ValueTask<ApiResult<Metrics>> PostMetrics(
            [FromServices] PipelineContext db,
            [FromServices] IMetricsManager metricsManager,
            [FromRoute] string projectId,
            [FromRoute] Guid agent,
            [FromBody] Metrics body,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult<Metrics>(401, "You don't have the permission to this project");
            }

            body.AgentId = agent;
            await metricsManager.PutAsync(body, cancellationToken);
            return ApiResult(body);
        }



        [HttpPost("{agent}/reboot")]
        public async ValueTask<ApiResult> Reboot(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] long pool,
            [FromRoute] Guid agent,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return ApiResult(401, "You don't have the permission to this project");
            }

            var _agent = await db.Agents
                .Where(x => x.AgentPoolId == pool)
                .Where(x => x.Id == agent)
                .SingleOrDefaultAsync(cancellationToken);

            using (var client = new HttpClient())
            {
                await client.PostAsync($"http://{_agent.Address}:5500/home/reboot", new StringContent(""));
            }

            _agent.Status = AgentStatus.Offline;

            await db.SaveChangesAsync();

            return ApiResult(200, "The agents has been rebooted");
        }

        /// <summary>
        /// Create an agent in specified pool
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pool">Pool ID</param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [HttpPost]
        [HttpPatch]
        public async ValueTask<ApiResult<Agent>> Post(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] long pool,
            [FromBody] Agent body,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult<Agent>(401, "You don't have the permission to this project");
            }

            body.AgentPoolId = pool;
            db.Agents.Add(body);
            await db.SaveChangesAsync();
            return ApiResult(body);
        }

        /// <summary>
        /// Change agent status
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="agent">Agent ID</param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{agent}")]
        [HttpPost("{agent}")]
        [HttpPatch("{agent}")]
        public async ValueTask<ApiResult<Agent>> Patch(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] Guid agent,
            [FromBody] Agent body,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult<Agent>(401, "You don't have the permission to this project");
            }

            var _agent = await db.Agents
                .SingleOrDefaultAsync(x => x.Id == agent, cancellationToken);

            if (_agent == null)
            {
                return ApiResult<Agent>(404, $"The agent '{agent}' has not been found");
            }

            _agent.Status = body.Status;

            await db.SaveChangesAsync();
            return ApiResult(body);
        }

        /// <summary>
        /// Delete an agent from pool
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="agent">Agent ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{agent}")]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] Guid agent,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult(401, "You don't have the permission to this project");
            }

            var _agent = await db.Agents
                .SingleOrDefaultAsync(x => x.Id == agent, cancellationToken);

            if (_agent == null)
            {
                return ApiResult(404, $"The agent '{agent}' has not been found");
            }

            db.Agents.Remove(_agent);
            await db.SaveChangesAsync();
            return ApiResult(200, "The agent has been removed successfully");
        }
    }
}
