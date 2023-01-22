// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Server.Hubs;
using Pomelo.DevOps.Server.LogManager;
using Pomelo.DevOps.Server.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Pomelo.DevOps.Shared;
using System.Reflection.Metadata.Ecma335;
using Pomelo.Workflow.Models;

namespace Pomelo.DevOps.Server.Controllers
{
    [Authorize]
    [Route("api/project/{projectId}/pipeline/{pipeline}/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        [HttpGet]
        public async ValueTask<PagedApiResult<Job>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromQuery] int p = 1,
            string name = null,
            string labels = null,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader))
            {
                return PagedApiResult<Job>(401, $"You don't have the permission to this pipeline");
            }

            var query = db.Jobs
                .Include(x => x.Labels)
                .Where(x => x.PipelineId == pipeline
                    && x.Pipeline.ProjectId == projectId);

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(labels))
            {
                var _labels = labels.Split(',').Select(x => x.Trim()).ToArray();
                query = query.Where(x => x.Labels.Any(y => _labels.Contains(y.Label)));
            }

            return await PagedApiResultAsync(query
                .OrderByDescending(x => x.TriggeredAt), p, 20, cancellationToken);
        }

        [HttpGet("{jobNumber}")]
        public async ValueTask<ApiResult<Job>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] int jobNumber,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader))
            {
                return ApiResult<Job>(401, $"You don't have the permission to this pipeline");
            }

            var _job = await db.Jobs
                .Include(x => x.Variables)
                .Include(x => x.LinearStages)
                .ThenInclude(x => x.Steps)
                .SingleOrDefaultAsync(x => x.Number == jobNumber
                    && x.PipelineId == pipeline, cancellationToken);

            if (_job == null)
            {
                return ApiResult<Job>(404, $"The pipeline job #{jobNumber} has not been found");
            }

            _job.LinearStages = _job.LinearStages.OrderBy(x => x.Order).ToList();
            foreach (var x in _job.LinearStages)
            {
                x.Steps = x.Steps.OrderBy(x => x.Order).ToList();
            }

            return ApiResult(_job);
        }

        [HttpPut]
        [HttpPost]
        [HttpPatch]
        public async ValueTask<ApiResult<Job>> Post(
            [FromServices] PipelineContext db,
            [FromServices] JobStateMachineFactory factory,
            [FromBody] PostPipelineJobRequest body,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<Job>(401, "You don't have permission to trigger this pipeline");
            }

            // Load the pipeline definition
            var _pipeline = await db.Pipelines
                .SingleOrDefaultAsync(x => x.Id == pipeline, cancellationToken);

            if (_pipeline == null)
            {
                return ApiResult<Job>(404, "The pipeline has not been found");
            }

            if (_pipeline.Type == PipelineType.Diagram)
            {
                // Check pipeline workflow
                if (!await db.WorkflowVersions.AnyAsync(x => x.Status == WorkflowVersionStatus.Available
                    && x.WorkflowId == _pipeline.WorkflowId, cancellationToken))
                {
                    return ApiResult<Job>(400, "The pipeline workflow is in-design, cannot start job.");
                }

                // Check stage workflow
                if (!await db.PipelineDiagramStages
                    .Where(x => x.PipelineId == pipeline)
                    .AllAsync(x => x.Workflow.Versions.Any(y => y.Status == WorkflowVersionStatus.Available), cancellationToken))
                {
                    return ApiResult<Job>(400, "Some of stage workflows are in-design, cannot start job.");
                }
            }

            // Prepare variables
            var variables = body.Arguments.Select(x => new JobVariable
            {
                Name = x.Key,
                Value = x.Value
            })
                .ToList();
            var stages = new List<JobStage>();
            var pipelineDefinition = YamlDeserializer.Deserialize<PipelineDefinition>(_pipeline.PipelineBody);
            var stageDefinitions = pipelineDefinition
                .Stages
                .OrderBy(x => x.Order);
            // Create stage instances from definition
            foreach (var x in stageDefinitions.OrderBy(x => x.Order))
            {
                for (var i = 0; i < x.AgentCount; ++i)
                {
                    stages.Add(new JobStage
                    {
                        Status = PipelineJobStatus.Pending,
                        Name = x.Name,
                        Identifier = null,
                        Condition = x.Condition,
                        ConditionVariable = x.ConditionVariable,
                        AgentPoolId = x.AgentPoolId,
                        Order = x.Order,
                        Timeout = x.Timeout,
                        IsolationLevel = x.IsolationLevel,
                        Steps = x.Steps // Create step instances
                            .OrderBy(y => y.Order)
                            .Select(y => new JobStep
                            {
                                Condition = y.Condition,
                                ConditionVariable = y.ConditionVariable,
                                Status = PipelineJobStatus.Pending,
                                StepId = y.StepId,
                                Name = y.Name,
                                Method = y.Method,
                                Timeout = y.Timeout,
                                Retry = y.Retry,
                                ErrorHandlingMode = y.ErrorHandlingMode,
                                Attempts = 0,
                                Version = y.Version,
                                Arguments = y.Arguments,
                                Order = y.Order
                            })
                            .ToList()
                    });
                }
            }

            var job = new Job
            {
                Status = PipelineJobStatus.Pending,
                Description = body.Description,
                PipelineId = pipeline,
                Timeout = pipelineDefinition.Timeout,
                TriggerType = body.TriggerType,
                TriggerName = body.TriggerName,
                Name = body.Name,
                Variables = variables
            };

            if (_pipeline.Type == PipelineType.Linear)
            {
                job.LinearStages = stages;
            }
            else 
            {
                // TODO: Generate workflow instance
            }

            job.Number = await db.Jobs.Where(x => x.PipelineId == pipeline).CountAsync() + 1;
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            var statemachine = factory.GetOrCreateStatemachine(job.Id);
            await statemachine.TransitAsync();
            return ApiResult(job);
        }

        /// <summary>
        /// Patch step status & times
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pipeline">Pipeline ID</param>
        /// <param name="job">Job ID</param>
        /// <param name="stage">Job Stage ID</param>
        /// <param name="step">Job Stage Step ID</param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{jobNumber}/stage/{stage}/step/{step}")]
        [HttpPost("{jobNumber}/stage/{stage}/step/{step}")]
        [HttpPatch("{jobNumber}/stage/{stage}/step/{step}")]
        public async ValueTask<ApiResult<JobStep>> PatchStep(
            [FromServices] PipelineContext db,
            [FromServices] JobStateMachineFactory factory,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromRoute] Guid stage,
            [FromRoute] Guid step,
            [FromBody] JobStep body,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<JobStep>(401, "You don't have permission to this pipeline job");
            }

            var _step = await db.JobSteps
                .Include(x => x.PipelineJobStage)
                .ThenInclude(x => x.PipelineJob)
                .SingleOrDefaultAsync(x => x.Id == step
                    && x.PipelineJobStageId == stage
                    && x.PipelineJobStage.PipelineJob.Number == jobNumber
                    && x.PipelineJobStage.PipelineJob.PipelineId == pipeline, cancellationToken);

            if (_step == null)
            {
                return ApiResult<JobStep>(404, "The step has not been found");
            }

            if (_step.PipelineJobStage.PipelineJob.Status == PipelineJobStatus.Skipped)
            {
                return ApiResult<JobStep>(400, "The job has not been aborted");
            }

            // Update the status of step
            _step.StartedAt = body.StartedAt;
            _step.FinishedAt = body.FinishedAt;
            var needTransition = false;
            if (_step.Status != body.Status) // Detect state change
            {
                needTransition = true;
            }
            _step.Status = body.Status;
            _step.Attempts = body.Attempts;
            await db.SaveChangesAsync();

            if (needTransition)
            {
                // Restore the statemachine to transit states
                var statemachine = factory.GetOrCreateStatemachine(_step.PipelineJobStage.PipelineJobId);
                await statemachine.TransitAsync();
            }

            return ApiResult(_step);
        }

        /// <summary>
        /// Patch a job stage
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pipeline">Pipeline</param>
        /// <param name="job">Job ID</param>
        /// <param name="stage">Job Stage ID</param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{jobNumber}/stage/{stage}")]
        [HttpPost("{jobNumber}/stage/{stage}")]
        [HttpPatch("{jobNumber}/stage/{stage}")]
        public async ValueTask<ApiResult> PatchStage(
            [FromServices] PipelineContext db,
            [FromServices] JobStateMachineFactory factory,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromRoute] Guid stage,
            [FromBody] JobStage body,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult(401, "You don't have permission to this pipeline job");
            }

            await db.JobStages
                .Where(x => x.PipelineJob.Number == jobNumber)
                .Where(x => x.PipelineJob.PipelineId == pipeline)
                .Where(x => x.Id == stage)
                .SetField(x => x.Status).WithValue(body.Status)
                .SetField(x => x.FinishedAt).WithValue(body.FinishedAt)
                .UpdateAsync();

            if (body.Status == PipelineJobStatus.Skipped
                || body.Status == PipelineJobStatus.Succeeded)
            {
                await db.JobSteps
                    .Where(x => x.PipelineJobStageId == stage)
                    .Where(x => x.Status <= PipelineJobStatus.Running)
                    .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                    .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                    .UpdateAsync();
            }
            else if (body.Status == PipelineJobStatus.Failed)
            {
                await db.JobSteps
                    .Where(x => x.PipelineJobStageId == stage)
                    .Where(x => x.Status <= PipelineJobStatus.Running)
                    .SetField(x => x.Status).WithValue(PipelineJobStatus.Failed)
                    .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                    .UpdateAsync();
            }

            await db.JobSteps
                .Where(x => x.PipelineJobStageId == stage)
                .Where(x => !x.StartedAt.HasValue)
                .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                .UpdateAsync();

            var statemachine = factory.GetOrCreateStatemachine(await GetJobIdByNumberAsync(db, pipeline, jobNumber));
            await statemachine.TransitAsync();

            return ApiResult(200, "Job stage has been patched successfully");
        }

        [HttpGet("{jobNumber}/log/{log}")]
        public async ValueTask<ApiResult<IEnumerable<Models.ViewModels.Log>>> GetLogs(
            [FromServices] ILogManager logManager,
            [FromServices] PipelineContext db,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromRoute] string log,
            CancellationToken cancellationToken = default)
        {
            return ApiResult(await logManager.GetLogsAsync(pipeline, jobNumber, log, cancellationToken));
        }

        [HttpPost("{jobNumber}/log/{log}")]
        public async ValueTask<ApiResult> PostLogs(
            [FromServices] ILogManager logManager,
            [FromServices] IHubContext<PipelineHub> hub,
            [FromBody] PostLogsRequest request,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromRoute] string log,
            CancellationToken cancellationToken = default)
        {
            await logManager.PutLogsAsync(pipeline, jobNumber, log, request.Logs, cancellationToken);
            foreach (var x in request.Logs)
            {
                await hub.Clients.Group($"pipeline-{pipeline}-job-{jobNumber}-log-{log}").SendAsync("logReceived", x.Text, x.Level.ToString(), x.Time);
            }
            return ApiResult(200, "Logs are saved");
        }

        [HttpPost("{jobNumber}/abort")]
        public async ValueTask<ApiResult> Abort(
            [FromServices] PipelineContext db,
            [FromServices] ILogManager logManager,
            [FromServices] IHubContext<PipelineHub> hub,
            [FromServices] JobStateMachineFactory factory,
            [FromRoute] long jobNumber,
            [FromRoute] string pipeline,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult(401, "You don't have permission to this pipeline job");
            }

            await db.Jobs
                .Where(x => x.PipelineId == pipeline)
                .Where(x => x.Number == jobNumber)
                .Where(x => x.Status <= PipelineJobStatus.Running)
                .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                .UpdateAsync();

            await db.JobStages
                .Where(x => x.PipelineJob.PipelineId == pipeline)
                .Where(x => x.PipelineJob.Number == jobNumber)
                .Where(x => x.Status <= PipelineJobStatus.Running)
                .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                .UpdateAsync();

            await db.JobStages
                .Where(x => x.PipelineJob.PipelineId == pipeline)
                .Where(x => x.PipelineJob.Number == jobNumber)
                .Where(x => !x.StartedAt.HasValue)
                .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                .UpdateAsync();

            await db.JobSteps
                .Where(x => x.PipelineJobStage.PipelineJob.PipelineId == pipeline)
                .Where(x => x.PipelineJobStage.PipelineJob.Number == jobNumber)
                .Where(x => x.Status <= PipelineJobStatus.Running)
                .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                .UpdateAsync();

            await db.JobSteps
                .Where(x => x.PipelineJobStage.PipelineJob.PipelineId == pipeline)
                .Where(x => x.PipelineJobStage.PipelineJob.Number == jobNumber)
                .Where(x => !x.StartedAt.HasValue)
                .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                .UpdateAsync();

            await hub.Clients.Group($"pipeline-{pipeline}-job-{jobNumber}").SendAsync("jobChanged");
            await hub.Clients.Group($"pipeline-{pipeline}-test-{jobNumber}").SendAsync("onTestCaseDeleted");
            var jobId = await GetJobIdByNumberAsync(db, pipeline, jobNumber);
            factory.DisposeStatemachine(jobId);
            return ApiResult(200, "The job has been aborted");
        }

        #region Job Extensions

        #endregion

        internal static async ValueTask<Guid> GetJobIdByNumberAsync(
            PipelineContext db,
            string pipelineId,
            long jobNumber,
            CancellationToken cancellationToken = default)
        {
            return await db.Jobs
                .Where(x => x.PipelineId == pipelineId && x.Number == jobNumber)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        #region Proxy
        [HttpGet("{jobNumber}/extensions/{id}/{*endpoint}")]
        public async ValueTask<IActionResult> GetProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            [FromRoute] string pipeline,
            [FromRoute] string jobNumber,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job Extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["X-Pomelo-Pipeline"] = pipeline,
                ["X-Pomelo-Job-Number"] = jobNumber,
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }

        [HttpPost("{jobNumber}/extensions/{id}/{*endpoint}")]
        public async ValueTask<IActionResult> PostProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            [FromRoute] string pipeline,
            [FromRoute] string jobNumber,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job Extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["X-Pomelo-Pipeline"] = pipeline,
                ["X-Pomelo-Job-Number"] = jobNumber,
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }

        [HttpPut("{jobNumber}/extensions/{id}/{*endpoint}")]
        public async ValueTask<IActionResult> PutProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            [FromRoute] string pipeline,
            [FromRoute] string jobNumber,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job Extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["X-Pomelo-Pipeline"] = pipeline,
                ["X-Pomelo-Job-Number"] = jobNumber,
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }

        [HttpPatch("{jobNumber}/extensions/{id}/{*endpoint}")]
        public async ValueTask<IActionResult> PatchProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            [FromRoute] string pipeline,
            [FromRoute] string jobNumber,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job Extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["X-Pomelo-Pipeline"] = pipeline,
                ["X-Pomelo-Job-Number"] = jobNumber,
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }

        [HttpDelete("{jobNumber}/extensions/{id}/{*endpoint}")]
        public async ValueTask<IActionResult> DeleteProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            [FromRoute] string pipeline,
            [FromRoute] string jobNumber,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job Extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["X-Pomelo-Pipeline"] = pipeline,
                ["X-Pomelo-Job-Number"] = jobNumber,
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }
        #endregion

        #region Misc


        [HttpPut("{jobNumber}/misc/rename-job")]
        [HttpPost("{jobNumber}/misc/rename-job")]
        [HttpPatch("{jobNumber}/misc/rename-job")]
        public async ValueTask<ApiResult> RenameJob(
            [FromServices] PipelineContext db,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromBody] RenameJobRequest request,
            CancellationToken cancellationToken = default)
        {
            var job = await db.Jobs
                .Include(x => x.Pipeline)
                .Include(x => x.LinearStages)
                .Where(x => x.Number == jobNumber && x.PipelineId == pipeline)
                .SingleOrDefaultAsync(cancellationToken);

            if (job == null)
            {
                return ApiResult(404, "The job has not been found");
            }

            if (!await HasPermissionToProjectAsync(db, job.Pipeline.ProjectId, ProjectMemberRole.Agent, cancellationToken))
            {
                return ApiResult(403, "No permission");
            }

            job.Name = request.Name;
            await db.SaveChangesAsync();
            return ApiResult(200, "The job name has been renamed to be " + request.Name);
        }

        [HttpPut("{jobNumber}/misc/add-label")]
        [HttpPost("{jobNumber}/misc/add-label")]
        [HttpPatch("{jobNumber}/misc/add-label")]
        public async ValueTask<ApiResult> AddJobLabel(
            [FromServices] PipelineContext db,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromBody] AddJobLabelRequest request,
            CancellationToken cancellationToken = default)
        {
            var job = await db.Jobs
                .Include(x => x.Pipeline)
                .Include(x => x.LinearStages)
                .Include(x => x.Labels)
                .Where(x => x.Number == jobNumber && x.PipelineId == pipeline)
                .SingleOrDefaultAsync(cancellationToken);

            if (job == null)
            {
                return ApiResult(404, "The job has not been found");
            }

            if (!await HasPermissionToProjectAsync(db, job.Pipeline.ProjectId, ProjectMemberRole.Agent, cancellationToken))
            {
                return ApiResult(403, "No permission");
            }

            if (job.Labels.Select(x => x.Label).Contains(request.Label))
            {
                return ApiResult(400, "The label is already added.");
            }

            db.JobLabels.Add(new JobLabel
            {
                JobId = job.Id,
                Label = request.Label
            });
            await db.SaveChangesAsync();
            return ApiResult(200, "The job label " + request.Label + " has been added.");
        }
        #endregion
    }
}
