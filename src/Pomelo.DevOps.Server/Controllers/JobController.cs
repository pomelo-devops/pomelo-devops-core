// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Pipelines.Sockets.Unofficial.Arenas;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Server.Hubs;
using Pomelo.DevOps.Server.LogManager;
using Pomelo.DevOps.Server.Utils;
using Pomelo.DevOps.Server.Workflow;
using Pomelo.DevOps.Shared;
using Pomelo.Workflow.Models;
using Pomelo.Workflow.Models.ViewModels;
using Pomelo.Workflow.Storage;

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
                return PagedApiResult<Job>(403, $"You don't have the permission to this pipeline");
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
                return ApiResult<Job>(403, $"You don't have the permission to this pipeline");
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
            [FromServices] DevOpsWorkflowManager wf,
            [FromServices] JobStateMachineFactory factory,
            [FromBody] PostPipelineJobRequest body,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<Job>(403, "You don't have permission to trigger this pipeline");
            }

            // Load the pipeline definition
            var _pipeline = await db.Pipelines
                .SingleOrDefaultAsync(x => x.Id == pipeline, cancellationToken);

            if (_pipeline == null)
            {
                return ApiResult<Job>(404, "The pipeline has not been found");
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
                Variables = variables,
                Type = _pipeline.Type
            };

            if (_pipeline.Type == PipelineType.Linear)
            {
                job.LinearStages = stages;
            }
            else 
            {
                // TODO: Generate workflow instance
                var latestVersion = await wf.GetLatestWorkflowVersionAsync(
                    _pipeline.WorkflowId.Value, 
                    WorkflowVersionStatus.Draft, 
                    cancellationToken);
                var arguments = body.Arguments
                    .Select(x => new KeyValuePair<string, JToken>(x.Key, JToken.FromObject(x.Value)))
                    .ToDictionary(x => x.Key, x => x.Value);
                var result = await wf.CreateNewWorkflowInstanceAsync(
                    _pipeline.WorkflowId.Value, 
                    latestVersion.Version, 
                    arguments, 
                    cancellationToken);
                job.DiagramWorkflowInstanceId = result.InstanceId;
            }

            job.Number = await db.Jobs.Where(x => x.PipelineId == pipeline).CountAsync() + 1;
            db.Jobs.Add(job);
            await db.SaveChangesAsync();
            if (job.Type == PipelineType.Linear)
            {
                var statemachine = factory.GetOrCreateStatemachine(job.Id);
                await statemachine.TransitAsync();
            }
            else
            {
                await wf.StartWorkflowInstanceAsync(job.DiagramWorkflowInstanceId.Value, cancellationToken);
            }
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
                return ApiResult<JobStep>(403, "You don't have permission to this pipeline job");
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
                return ApiResult(403, "You don't have permission to this pipeline job");
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
                return ApiResult(403, "You don't have permission to this pipeline job");
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

        #region Diagram

        [HttpGet("{jobNumber}/diagram")]
        public async ValueTask<ApiResult<InstanceDiagram>> GetJobDiagram(
            [FromServices] PipelineContext db,
            [FromServices] DevOpsWorkflowManager wf,
            [FromRoute] long jobNumber,
            [FromRoute] string pipeline,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<InstanceDiagram>(403, "You don't have permission to this pipeline job");
            }

            var job = await db.Jobs
                .FirstOrDefaultAsync(x => x.PipelineId == pipeline && x.Number == jobNumber, cancellationToken);

            if (job == null) 
            {
                return ApiResult<InstanceDiagram>(404, "The specified job was not found");
            }

            if (job.Type != PipelineType.Diagram)
            {
                return ApiResult<InstanceDiagram>(404, $"{job.Type} job does not support export a job diagram");
            }

            var instanceId = job.DiagramWorkflowInstanceId;
            var diagram = await wf.GetInstanceDiagramAsync(instanceId.Value, cancellationToken);
            return ApiResult(diagram);
        }
        #endregion

        #region Job Stage Workflows
        [HttpGet("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}")]
        public async ValueTask<ApiResult<WorkflowInstance>> GetStageWorkflowInstance(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromRoute] Guid workflowInstanceId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<WorkflowInstance>(403, "You don't have permission to this pipeline job");
            }

            var stage = await db.JobStageWorkflows
                .Include(x => x.WorkflowInstance)
                .Where(x => x.JobStage.Type == PipelineType.Diagram 
                    && x.JobStage.PipelineJob.PipelineId == pipeline 
                    && x.JobStage.PipelineJob.Number == jobNumber)
                .Where(x => x.WorkflowInstanceId == workflowInstanceId)
                .FirstOrDefaultAsync(cancellationToken);

            return ApiResult(stage.WorkflowInstance as WorkflowInstance);
        }

        [HttpGet("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/workflow-version")]
        public async ValueTask<ApiResult<WorkflowVersion>> GetStageWorkflowInstanceWorkflowVersion(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid workflowInstanceId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult<WorkflowVersion>(403, "You don't have permission to this pipeline job");
            }

            var instance = await wfs.GetWorkflowInstanceAsync(
                workflowInstanceId, 
                cancellationToken);

            var result = await wfs.GetWorkflowVersionAsync(
                instance.WorkflowId, 
                instance.WorkflowVersion, 
                cancellationToken);

            return ApiResult(result);
        }

        [HttpPost("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/connection")]
        public async ValueTask<ApiResult> PostStageWorkflowInstanceConnection(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid workflowInstanceId,
            [FromBody] PostStorageWorkflowInstanceConnectionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult(403, "You don't have permission to this pipeline job");
            }

            await wfs.CreateWorkflowInstanceConnectionAsync(new WorkflowInstanceConnection 
            {
                ConnectPolylineId = request.ConnectPolylineId,
                InstanceId = workflowInstanceId
            }, cancellationToken);
            return ApiResult(200, "Workflow step connection created");
        }

        [HttpPost("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/step")]
        public async ValueTask<ApiResult<Guid>> PostStageWorkflowInstanceStep(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid workflowInstanceId,
            [FromBody] WorkflowInstanceStep request, 
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult<Guid>(403, "You don't have permission to this pipeline job");
            }

            var result = await wfs.CreateWorkflowStepAsync(workflowInstanceId, request, cancellationToken);
            return ApiResult(result);
        }

        [HttpGet("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/step")]
        public async ValueTask<ApiResult<List<WorkflowInstanceStep>>> GetStageWorkflowInstanceSteps(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid workflowInstanceId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<List<WorkflowInstanceStep>>(403, "You don't have permission to this pipeline job");
            }

            var result = await wfs.GetInstanceStepsAsync(workflowInstanceId, cancellationToken);
            return ApiResult(result.ToList());
        }

        [HttpGet("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/misc/previous-steps")]
        public async ValueTask<ApiResult<GetPreviousStepsResult>> GetStageWorkflowInstancePreviousSteps(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromQuery] Guid stepId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<GetPreviousStepsResult>(403, "You don't have permission to this pipeline job");
            }

            var result = await wfs.GetPreviousStepsAsync(stepId, cancellationToken);
            return ApiResult(result);
        }

        [HttpGet("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/misc/get-step-by-shape-id")]
        public async ValueTask<ApiResult<WorkflowInstanceStep>> GetStageWorkflowInstanceStepByShapeId(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid workflowInstanceId,
            [FromQuery] Guid shapeId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<WorkflowInstanceStep>(403, "You don't have permission to this pipeline job");
            }

            var result = await wfs.GetStepByShapeId(workflowInstanceId, shapeId, cancellationToken);
            return ApiResult(result);
        }

        [HttpGet("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/connection")]
        public async ValueTask<ApiResult<List<WorkflowInstanceConnection>>> GetStageWorkflowInstanceConnections(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid workflowInstanceId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<List<WorkflowInstanceConnection>>(403, "You don't have permission to this pipeline job");
            }

            var result = await wfs.GetWorkflowInstanceConnectionsAsync(workflowInstanceId, cancellationToken);
            return ApiResult(result.ToList());
        }

        [HttpGet("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/step/{stepId:Guid}")]
        public async ValueTask<ApiResult<WorkflowInstanceStep>> GetStageWorkflowInstanceStep(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute]Guid stepId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<WorkflowInstanceStep>(403, "You don't have permission to this pipeline job");
            }

            var result = await wfs.GetWorkflowInstanceStepAsync(stepId, cancellationToken);
            return ApiResult(result);
        }

        [HttpPatch("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}")]
        public async ValueTask<ApiResult<UpdateWorkflowInstanceResult>> PatchStageWorkflowInstance(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid workflowInstanceId,
            [FromBody] PatchWorkflowInstanceRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<UpdateWorkflowInstanceResult>(403, "You don't have permission to this pipeline job");
            }

            var result = await wfs.UpdateWorkflowInstanceAsync(
                workflowInstanceId, 
                request.Status, 
                PatchArguments(request.Arguments), 
                cancellationToken);

            return ApiResult(result);
        }

        [HttpPatch("{jobNumber}/diagram-stage/{workflowInstanceId:Guid}/step/{stepId:Guid}")]
        public async ValueTask<ApiResult<UpdateWorkflowStepResult>> PatchStageWorkflowInstanceStep(
            [FromServices] PipelineContext db,
            [FromServices] IWorkflowStorageProvider wfs,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid stepId,
            [FromBody] PatchWorkflowInstanceStepRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<UpdateWorkflowStepResult>(403, "You don't have permission to this pipeline job");
            }

            var result = await wfs.UpdateWorkflowStepAsync(
                stepId, 
                request.Status, 
                PatchArguments(request.Arguments), 
                request.Error, 
                cancellationToken);

            return ApiResult(result);
        }

        private Action<Dictionary<string, JToken>> PatchArguments(Dictionary<string, JToken> args)
        {
            return new Action<Dictionary<string, JToken>>((arguments) =>
            {
                foreach (var x in args)
                {
                    arguments[x.Key] = x.Value;
                }
            });
        }
        #endregion

        #region Job Extensions

        #endregion

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
