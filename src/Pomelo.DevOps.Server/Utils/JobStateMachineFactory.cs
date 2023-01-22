// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Pomelo.DevOps.Server.Controllers;
using Pomelo.DevOps.Server.Hubs;
using Pomelo.DevOps.Server.LogManager;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pomelo.DevOps.Models.ViewModels;

namespace Pomelo.DevOps.Server.Utils
{
    public class JobStateMachineFactory
    {
        private IServiceProvider _services;
        private ConcurrentDictionary<Guid, JobStateMachine> _cache;

        public JobStateMachineFactory(IServiceProvider services)
        {
            _services = services;
            _cache = new ConcurrentDictionary<Guid, JobStateMachine>();
        }

        public JobStateMachine GetOrCreateStatemachine(Guid jobId)
        {
            return _cache.GetOrAdd(jobId, jobId =>
            {
                var scope = _services.CreateScope();
                return new JobStateMachine(
                    _services,
                    jobId,
                    this,
                    scope.ServiceProvider.GetRequiredService<AgentPoolController>(),
                    scope.ServiceProvider.GetRequiredService<JobController>(),
                    scope.ServiceProvider.GetRequiredService<ILogManager>(),
                    scope.ServiceProvider.GetRequiredService<IHubContext<PipelineHub>>());
            });
        }

        public void DisposeStatemachine(Guid jobId)
        {
            _cache.Remove(jobId, out var statemachine);
            statemachine?.Dispose();
        }
    }

    public class JobStateMachine : IDisposable
    {
        private IServiceProvider _services;
        private Guid _jobId;
        private List<JobStage> _stages;
        private JobStateMachineFactory _factory;
        private AgentPoolController _poolController;
        private JobController _jobController;
        private ILogManager _logManager;
        private Job job;
        private IHubContext<PipelineHub> _hub;

        public JobStateMachine(
            IServiceProvider services,
            Guid jobId,
            JobStateMachineFactory factory,
            AgentPoolController poolController,
            JobController jobController,
            ILogManager logManager,
            IHubContext<PipelineHub> hub)
        {
            _services = services;
            _factory = factory;
            _jobId = jobId;
            _poolController = poolController;
            _jobController = jobController;
            _logManager = logManager;
            _hub = hub;

            using var scope = _services.CreateScope();
            using var _db = scope.ServiceProvider.GetRequiredService<PipelineContext>();
            GetJobAsync().GetAwaiter().GetResult();
            if (job.Status >= PipelineJobStatus.Failed)
            {
                throw new InvalidOperationException($"The job {jobId} is already finished");
            }

            var pipelineId = job.PipelineId;
            _stages = _db.JobStages
                .Include(x => x.PipelineJob)
                .OrderBy(x => x.Order)
                .ToList();
        }

        public async ValueTask<bool> TransitAsync()
        {
            using var scope = _services.CreateScope();
            using var _db = scope.ServiceProvider.GetRequiredService<PipelineContext>();

            await GetJobAsync();
            if (_stages.Count == 0 
                || job.Status == PipelineJobStatus.Skipped
                || job.LinearStages.All(x => x.Status >= PipelineJobStatus.Failed))
            {
                await OnJobFinishedAsync();
                return true;
            }

            if (job.Status == PipelineJobStatus.Skipped)
            {
                return true;
            }

            await HandleCurrentStageResultAsync();
            if (!FindNextStage(out var nextStageOrder))
            {
                // Current stage has not finished.
                return false;
            }

            if (nextStageOrder.HasValue)
            {
                // Move next
                await _db.Jobs
                    .Where(x => x.Id == job.Id)
                    .Where(x => x.CurrentStageOrder < nextStageOrder.Value)
                    .SetField(x => x.CurrentStageOrder).WithValue(nextStageOrder.Value)
                    .UpdateAsync();

                if (await StartStageAsync())
                {
                    await TransitAsync();
                }
            }
            else
            {
                // Job finished
                await OnJobFinishedAsync();
            }

            return true;
        }

        private async ValueTask GetJobAsync()
        {
            using var scope = _services.CreateScope();
            using var _db = scope.ServiceProvider.GetRequiredService<PipelineContext>();
            job = await _db.Jobs
                .AsNoTracking()
                .Include(x => x.LinearStages)
                .ThenInclude(x => x.Steps)
                .Include(x => x.LinearStages)
                .SingleAsync(x => x.Id == _jobId);
        }

        private async ValueTask HandleCurrentStageResultAsync()
        {
            using var scope = _services.CreateScope();
            using var _db = scope.ServiceProvider.GetRequiredService<PipelineContext>();

            if (job.CurrentStageOrder == 0)
            {
                return;
            }

            var stages = job.LinearStages.Where(x => x.Order == job.CurrentStageOrder);
            foreach (var stage in stages)
            {
                if (stage.Status >= PipelineJobStatus.Failed)
                {
                    continue;
                }

                if (stage.Steps.Any(x => x.Status < PipelineJobStatus.Failed))
                {
                    continue;
                }
                else if (stage.Steps.All(x => x.Status >= PipelineJobStatus.Skipped))
                {
                    var message = "All steps were executed successfully, changing the stage status to succeeded.";
                    var level = LogLevel.Information;
                    await _logManager.PutLogAsync(job.PipelineId, job.Number, "stage-" + stage.Id, message, level);
                    await _hub.Clients.Group($"pipeline-{job.PipelineId}-job-{job.Number}-log-stage-{stage.Id}").SendAsync("logReceived", message, level.ToString(), DateTime.UtcNow);
                    await _db.JobStages
                        .Where(x => x.Id == stage.Id)
                        .Where(x => x.Status < PipelineJobStatus.Succeeded)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Succeeded)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();
                }
                else
                {
                    var message = "Some of steps in current stage got failed, changing the stage status to failed.";
                    var level = LogLevel.Information;
                    await _logManager.PutLogAsync(job.PipelineId, job.Number, "stage-" + stage.Id, message, level);
                    await _hub.Clients.Group($"pipeline-{job.PipelineId}-job-{job.Number}-log-stage-{stage.Id}").SendAsync("logReceived", message, level.ToString(), DateTime.UtcNow);
                    await _db.JobStages
                        .Where(x => x.Id == stage.Id)
                        .Where(x => x.Status < PipelineJobStatus.Failed)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Failed)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();
                }

                await GetJobAsync();
            }
            await _hub.Clients.Group($"pipeline-{job.PipelineId}-job-{job.Number}").SendAsync("jobChanged");
        }

        private bool FindNextStage(out int? nextOrder)
        {
            if (job.CurrentStageOrder == 0)
            {
                var stage = _stages.FirstOrDefault();
                nextOrder = stage == null ? null : (int?)1;
                return true;
            }

            var stages = job.LinearStages.Where(x => x.Order == job.CurrentStageOrder); // Find current stages
            if (stages.Any(x => x.Status < PipelineJobStatus.Failed))
            {
                // Stages not finished
                nextOrder = null;
                return false;
            }
            else
            {
                // Stages finished
                var nextStages = _stages.Where(x => x.Order == job.CurrentStageOrder + 1);
                if (nextStages.Count() == 0)
                {
                    nextOrder = null;
                    return true;
                }
                else
                {
                    nextOrder = job.CurrentStageOrder + 1;
                    return true;
                }
            }
        }

        private async ValueTask<bool> StartStageAsync()
        {
            using var scope = _services.CreateScope();
            using var _db = scope.ServiceProvider.GetRequiredService<PipelineContext>();

            var needTransitAgain = false;
            await GetJobAsync();
            var stages = job.LinearStages.Where(x => x.Order == job.CurrentStageOrder);

            List<JobVariable> variables = null;

            variables = await _db.JobVariables
                .Where(x => x.PipelineJobId == job.Id)
                .ToListAsync();

            foreach (var x in stages)
            {
                var prevStages = job.LinearStages.Where(x => x.Order == job.CurrentStageOrder - 1);
                if (x.AgentPoolId == null)
                {
                    needTransitAgain = true;
                    await _db.JobStages
                        .Where(y => y.Id == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Failed)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Failed)
                        .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();

                    await _db.JobSteps
                        .Where(y => y.PipelineJobStageId == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Failed)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Failed)
                        .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();


                    foreach (var y in x.Steps)
                    {
                        await _logManager.PutLogAsync(job.PipelineId, job.Number, "step-" + y.Id, "The step has been cancelled because its stage has not been specified a correct agent pool.", LogLevel.Error);
                    }

                    await _logManager.PutLogAsync(job.PipelineId, job.Number, "stage-" + x.Id, "The stage has not been specified a correct agent pool.", LogLevel.Error);
                }
                else if (x.Condition == RunCondition.CheckVariable && !variables.Any(y => y.Name == x.ConditionVariable && y.Value.ToLower() == "true"))
                {
                    needTransitAgain = true;
                    await _db.JobSteps
                        .Where(y => y.PipelineJobStageId == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Skipped)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                        .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();

                    await _db.JobSteps
                        .Where(y => y.PipelineJobStageId == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Skipped)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                        .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();

                    foreach (var y in x.Steps)
                    {
                        await _logManager.PutLogAsync(job.PipelineId, job.Number, "step-" + y.Id, "The stage condition variable is not true, skipped all steps of this stage", LogLevel.Information);
                    }

                    await _logManager.PutLogAsync(job.PipelineId, job.Number, "stage-" + x.Id, "The condition variable is not true, skipped this stage", LogLevel.Information);
                }
                else if (x.Condition == RunCondition.RequirePreviousTaskFailed && prevStages.All(x => x.Status != PipelineJobStatus.Failed))
                {
                    needTransitAgain = true;
                    await _db.JobSteps
                        .Where(y => y.PipelineJobStageId == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Skipped)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                        .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();


                    await _db.JobSteps
                        .Where(y => y.PipelineJobStageId == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Skipped)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                        .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();

                    foreach (var y in x.Steps)
                    {
                        await _logManager.PutLogAsync(job.PipelineId, job.Number, "step-" + y.Id, "The step has been cancelled because the previous stages are not failed.", LogLevel.Information);
                    }
                }
                else if (x.Condition == RunCondition.RequirePreviousTaskSuccess && prevStages.Any(x => x.Status != PipelineJobStatus.Succeeded))
                {
                    needTransitAgain = true;
                    await _db.JobSteps
                        .Where(y => y.PipelineJobStageId == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Skipped)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                        .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();

                    await _db.JobSteps
                        .Where(y => y.PipelineJobStageId == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Skipped)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Skipped)
                        .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                        .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                        .UpdateAsync();

                    foreach (var y in x.Steps)
                    {
                        await _logManager.PutLogAsync(job.PipelineId, job.Number, "step-" + y.Id, "The step has been cancelled because the previous stages are not failed.", LogLevel.Information);
                    }
                }
                else
                {
                    await _db.JobStages
                        .Where(y => y.Id == x.Id)
                        .Where(x => x.Status < PipelineJobStatus.Waiting)
                        .SetField(x => x.Status).WithValue(PipelineJobStatus.Waiting)
                        .UpdateAsync();
                    
                    AgentPool pool = null;
                    pool = await _db.AgentPools.SingleAsync(y => y.Id == x.AgentPoolId);
                    await _logManager.PutLogAsync(job.PipelineId, job.Number, "stage-" + x.Id, "Starting stage...", LogLevel.Information);
                }

                await GetJobAsync();
            }

            return needTransitAgain;
        }

        private async ValueTask OnJobFinishedAsync()
        {
            using var scope = _services.CreateScope();
            using var _db = scope.ServiceProvider.GetRequiredService<PipelineContext>();

            if (job.Status == PipelineJobStatus.Skipped)
            {
                Dispose();
                return;
            }
            else if (job.LinearStages.Count == 0 
                || job.LinearStages.All(x => x.Status == PipelineJobStatus.Skipped 
                    || x.Status == PipelineJobStatus.Succeeded))
            {
                await _db.Jobs
                    .Where(x => x.Id == job.Id)
                    .Where(x => x.Status < PipelineJobStatus.Succeeded)
                    .SetField(x => x.Status).WithValue(PipelineJobStatus.Succeeded)
                    .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                    .UpdateAsync();
            }
            else
            {
                await _db.Jobs
                    .Where(x => x.Id == job.Id)
                    .Where(x => x.Status < PipelineJobStatus.Failed)
                    .SetField(x => x.Status).WithValue(PipelineJobStatus.Failed)
                    .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                    .UpdateAsync();
            }

            
            await _hub.Clients.Group($"pipeline-{job.PipelineId}-job-{job.Number}").SendAsync("jobChanged");
            Dispose();
        }

        public void Dispose()
        {
            _factory.DisposeStatemachine(_jobId);
        }
    }

    public static class JobStateMachineFactoryExtensions
    {
        public static IServiceCollection AddJobStateMachineFactory(this IServiceCollection collection)
        {
            return collection.AddSingleton(x => new JobStateMachineFactory(x));
        }
    }
}
