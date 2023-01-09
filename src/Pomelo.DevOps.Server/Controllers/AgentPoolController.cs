// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Server.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;

namespace Pomelo.DevOps.Server.Controllers
{
    [Authorize]
    [Route("api/project/{projectId}/[controller]")]
    [ApiController]
    public class AgentPoolController : ControllerBase
    {
        /// <summary>
        /// Get all agent pools from project
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async ValueTask<ApiResult<List<AgentPool>>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default)
        { 
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult<List<AgentPool>>(401, "You don't have the permission to get agent pools from this project");
            }

            return ApiResult(await db.AgentPools
                .Include(x => x.Agents)
                .Where(x => x.ProjectId == projectId)
                .ToListAsync(cancellationToken));
        }

        /// <summary>
        /// Get an agent pool detail
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pool">Pool ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{pool}")]
        public async ValueTask<ApiResult<AgentPool>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] long pool,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult<AgentPool>(401, "You don't have the permission to get agent pools from this project");
            }

            var _pool = await db.AgentPools
                .Include(x => x.Agents)
                .ThenInclude(x => x.CurrentJobStage)
                .ThenInclude(x => x.PipelineJob)
                .ThenInclude(x => x.Pipeline)
                .Where(x => x.ProjectId == projectId)
                .Where(x => x.Id == pool)
                .SingleOrDefaultAsync(cancellationToken);

            if (_pool == null)
            {
                return ApiResult<AgentPool>(404, "Agent pool not found");
            }

            return ApiResult(_pool);
        }

        /// <summary>
        /// Create an agent pool
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId"></param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async ValueTask<ApiResult<AgentPool>> Post(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromBody] AgentPool body,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult<AgentPool>(401, "You don't have the permission to get agent pools from this project");
            }

            body.ProjectId = projectId;
            db.AgentPools.Add(body);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(body);
        }

        /// <summary>
        /// Update an agent pool
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pool">Pool ID</param>
        /// <param name="jobController"></param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{pool}")]
        [HttpPatch("{pool}")]
        public async ValueTask<ApiResult<AgentPool>> Put(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] long pool,
            [FromBody] AgentPool body,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult<AgentPool>(401, "You don't have the permission to get agent pools from this project");
            }

            var _pool = await db.AgentPools
                .Where(x => x.ProjectId == projectId)
                .Where(x => x.Id == pool)
                .SingleOrDefaultAsync(cancellationToken);

            if (_pool == null)
            {
                return ApiResult<AgentPool>(404, "Specified pool was not found");
            }

            _pool.Name = body.Name;
            await db.SaveChangesAsync();
            return ApiResult(_pool);
        }

        [HttpPost("{pool}/reboot")]
        public async ValueTask<ApiResult> Reboot(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] long pool,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult(401, "You don't have the permission to get agent pools from this project");
            }

            var _pool = await db.AgentPools
                .Include(x => x.Agents)
                .Where(x => x.ProjectId == projectId)
                .Where(x => x.Id == pool)
                .SingleOrDefaultAsync(cancellationToken);

            using (var client = new HttpClient())
            {
                await Task.WhenAll(_pool.Agents.Select(x => client.PostAsync($"http://{x.Address}:5500/home/reboot", new StringContent(""))));
            }

            foreach(var x in _pool.Agents)
            {
                x.Status = AgentStatus.Offline;
            }

            await db.SaveChangesAsync();

            return ApiResult(200, "The agents has been rebooted");
        }

        /// <summary>
        /// Remove an agent pool
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pool">Pool ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{pool}")]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] long pool,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult(401, "You don't have the permission to get agent pools from this project");
            }

            await db.Agents
                .Where(x => x.AgentPoolId == pool)
                .DeleteAsync();

            await db.AgentPools
                .Where(x => x.Id == pool)
                .DeleteAsync();

            return ApiResult(200, $"The agent pool #{pool} has been removed successfully");
        }
    }
}
