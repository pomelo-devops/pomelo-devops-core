// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Server.Utils;
using Pomelo.DevOps.Server.Workflow;

namespace Pomelo.DevOps.Server.Controllers
{
    // Please puts some unstable actions here
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MiscController : ControllerBase
    {
        [HttpPost("execute-job")]
        public async ValueTask<IActionResult> ExecuteJob(
            [FromServices] PipelineContext db,
            [FromServices] DevOpsWorkflowManager wf,
            [FromBody] ExecuteJobRequest body,
            CancellationToken cancellationToken = default)
        {
            if (!await DetermineAgentVersionAsync(cancellationToken))
            {
                return StatusCode(412, "Agent needs to be upgraded");
            }

            var agent = await db.Agents
                .Include(x => x.AgentPool)
                .SingleOrDefaultAsync(x => x.Id == body.AgentId);

            if (agent == null)
            {
                return Forbid();
            }

            if (!await HasPermissionToProjectAsync(db, agent.AgentPool.ProjectId, ProjectMemberRole.Agent, cancellationToken))
            {
                return Forbid();
            }

            var query = db.JobStages
                .Include(x => x.PipelineJob)
                .ThenInclude(x => x.Pipeline)
                .Include(x => x.Steps)
                .Include(x => x.PipelineDiagramStage)
                .Include(x => x.JobStageWorkflow)
                .Where(x => x.PipelineJob.Status != PipelineJobStatus.Skipped)
                .Where(x => x.AgentPoolId == agent.AgentPoolId)
                .Where(x => x.Status == PipelineJobStatus.Waiting
                        || x.Status == PipelineJobStatus.Running && x.AgentId == body.AgentId && !body.ProhibitStages.Contains(x.Id))
                .Where(x => x.PipelineJob.Type == PipelineType.Linear 
                    || x.PipelineJob.Type == PipelineType.Diagram 
                        && x.PipelineJob.Pipeline.Workflow.Versions.Any(y => y.Status == Pomelo.Workflow.Models.WorkflowVersionStatus.Available)
                        && x.PipelineJob.Pipeline.Stages.All(y => y.Workflow.Versions.Any(z => z.Status == Pomelo.Workflow.Models.WorkflowVersionStatus.Available)));

            if (body.IsParallel)
            {
                query = query
                    .Where(x => x.IsolationLevel == IsolationLevel.Parallel);
            }
            else
            {
                query = query
                    .Where(x => x.IsolationLevel == IsolationLevel.Sequential);
            }

            query = query
                .OrderBy(x => x.PipelineJob.TriggeredAt)
                .ThenBy(x => x.PipelineJobId);

            lock (JobLocker)
            {
                var stage = query
                    .FirstOrDefault();

                agent.HeartBeat = DateTime.UtcNow;
                agent.Address = this.GetRemoteIPAddress().Address.AddressFamily == AddressFamily.InterNetworkV6 
                    ? $"[{this.GetRemoteIPAddress().Address}]" 
                    : this.GetRemoteIPAddress().Address.ToString();
                agent.ClientVersion = Request.Headers.ContainsKey("X-Pomelo-Agent-Version")
                    ? Request.Headers["X-Pomelo-Agent-Version"].FirstOrDefault()
                    : null;

                if (stage == null)
                {
                    agent.Status = AgentStatus.Idle;
                    db.SaveChanges();
                    return NotFound();
                }

                if (!HasPermissionToPipelineAsync(db, stage.PipelineJob.Pipeline.ProjectId, stage.PipelineJob.PipelineId, PipelineAccessType.Collaborator, cancellationToken).Result)
                {
                    return Forbid();
                }
                
                db.Jobs
                    .Where(x => x.Id == stage.PipelineJobId)
                    .Where(x => x.Status <= PipelineJobStatus.Waiting)
                    .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                    .SetField(x => x.Status).WithValue(PipelineJobStatus.Running)
                    .Update();

                stage.Status = PipelineJobStatus.Running;
                if (stage.StartedAt == null)
                {
                    stage.StartedAt = DateTime.UtcNow;
                }
                stage.AgentId = body.AgentId;
                agent.Status = AgentStatus.Busy;
                stage.JobNumber = stage.PipelineJob.Number;
                stage.Project = stage.PipelineJob.Pipeline.ProjectId;
                stage.Pipeline = stage.PipelineJob.PipelineId;
                if (stage.PipelineJob.Pipeline.Type == PipelineType.Linear)
                {
                    stage.Type = PipelineType.Linear;
                    stage.Steps = stage.Steps.OrderBy(x => x.Order).ToList();
                    stage.Diagram = null;
                }
                else
                {
                    var workflowVersion = db.WorkflowVersions
                        .First(x => x.WorkflowId == stage.PipelineDiagramStage.WorkflowId
                            && x.Status == Pomelo.Workflow.Models.WorkflowVersionStatus.Available);

                    stage.Type = PipelineType.Diagram;
                    stage.Steps = null;
                    stage.WorkflowInstanceId = stage.JobStageWorkflow.WorkflowInstanceId;
                }
                db.SaveChanges();
                return Content(YamlSerializer.Serialize(stage), "application/yaml");
            }
        }

        [HttpPost("convert-pipeline-to-yaml")]
        public string ConvertPipelineToYaml(
            [FromBody] PutPipelineRequest body)
        {
            Response.ContentType = "text/plain";
            return YamlSerializer.Serialize(body);
        }

        [HttpPost("convert-pipeline-yaml-to-json")]
        public GetPipelineResponse ConvertPipelineYamlToJson(
            [FromBody] ConvertPipelineYamlToJsonRequest request)
        {
            var yaml = request.Yaml;
            return YamlDeserializer.Deserialize<GetPipelineResponse>(yaml);
        }

        [HttpPut("job-stage-pool")]
        [HttpPost("job-stage-pool")]
        [HttpPatch("job-stage-pool")]
        public async ValueTask<ApiResult> PatchPipelineJobStagePool(
            [FromServices] PipelineContext db,
            [FromBody] PatchStagePoolRequest request,
            CancellationToken cancellationToken = default)
        {
            var job = await db.Jobs
                .Include(x => x.Pipeline)
                .Include(x => x.LinearStages)
                .Where(x => x.Id == request.JobId)
                .SingleOrDefaultAsync(cancellationToken);

            if (job == null)
            {
                return ApiResult(404, "The job has not been found");
            }

            if (!await HasPermissionToProjectAsync(db, job.Pipeline.ProjectId, ProjectMemberRole.Agent, cancellationToken))
            {
                return ApiResult(403, "No permission");
            }

            foreach (var stage in job.LinearStages.Where(x => x.Name == request.StageName))
            {
                stage.AgentPoolId = request.AgentPool;
            }

            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(200, "The agent pool has been assigned to stages successfully");
        }

        [HttpGet("check-pipeline-id-available")]
        public async ValueTask<ApiResult<bool>> GetCheckPipelineIdAvailable(
            [FromServices] PipelineContext db,
            [FromQuery] string pipelineId,
            CancellationToken cancellationToken = default)
        {
            return ApiResult(!await db.Pipelines.AnyAsync(x => x.Id == pipelineId, cancellationToken));
        }

        [HttpPost("batch-query-widgets")]
        public async ValueTask<ApiResult<List<WidgetBase>>> PostBatchQueryWidgets(
            [FromServices] PipelineContext db,
            [FromBody] GetWidgetEntriesRequest request,
            CancellationToken cancellationToken = default)
        {
            request.WidgetIds = request.WidgetIds.Distinct();
            var widgets = await db.Widgets
                .Where(x => request.WidgetIds.Contains(x.Id))
                .Select(x => new WidgetBase
                {
                    Id = x.Id,
                    ConfigEntryView = x.ConfigEntryView,
                    Description = x.Description,
                    IconUrl = x.IconUrl,
                    Name = x.Name,
                    WidgetEntryView = x.WidgetEntryView
                })
                .ToListAsync(cancellationToken);

            return ApiResult(widgets);
        }
    }
}
