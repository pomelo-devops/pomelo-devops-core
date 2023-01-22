// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Server.LogManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.DevOps.Server.Utils
{
    public class TimeKeeper : IDisposable
    {
        private IServiceScope _scope;
        private PipelineContext _db;
        private ILogManager _log;
        private JobStateMachineFactory _factory;

        public TimeKeeper(IServiceProvider services)
        {
            _scope = services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<PipelineContext>();
            _log = _scope.ServiceProvider.GetRequiredService<ILogManager>();
            _factory = _scope.ServiceProvider.GetRequiredService<JobStateMachineFactory>();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public async ValueTask StartMonitorAsync(CancellationToken cancellationToken = default)
        {
            while(true)
            {
                await CollectJobAsync(cancellationToken);
                await CollectStageAsync(cancellationToken);
                await Task.Delay(60000);
            }
        }

        private async ValueTask CollectJobAsync(CancellationToken cancellationToken = default)
        {
            var jobs = await _db.Jobs
                .AsNoTracking()
                .Include(x => x.LinearStages)
                .ThenInclude(x => x.Steps)
                .Where(x => x.Status == PipelineJobStatus.Running)
                .Where(x => x.Timeout != -1)
                .Where(x => x.TriggeredAt.AddSeconds(x.Timeout) < DateTime.UtcNow)
                .ToListAsync();

            foreach (var job in jobs)
            {
                try
                {
                    await SetJobTimeoutAsync(job, _db, cancellationToken);
                }
                catch { }
            }
        }

        private async ValueTask CollectStageAsync(CancellationToken cancellationToken = default)
        {
            var stages = await _db.JobStages
                .AsNoTracking()
                .Include(x => x.PipelineJob)
                .Include(x => x.Steps)
                .Where(x => x.StartedAt.HasValue)
                .Where(x => x.Timeout != -1)
                .Where(x => x.StartedAt.Value.AddSeconds(x.Timeout) < DateTime.UtcNow)
                .Where(x => x.Status == PipelineJobStatus.Running)
                .ToListAsync();

            foreach (var stage in stages)
            {
                try
                {
                    await SetStageTimeoutAsync(stage, true, cancellationToken);
                }
                catch { }
            }
        }

        private async ValueTask SetJobTimeoutAsync(
            Job job,
            PipelineContext db,
            CancellationToken cancellationToken = default)
        {
            job.FinishedAt = DateTime.UtcNow;
            job.Status = PipelineJobStatus.Failed;
            if (job.LinearStages != null)
            {
                foreach (var x in job.LinearStages)
                {
                    x.PipelineJob = job;
                    await SetStageTimeoutAsync(x, false, cancellationToken);
                }
            }

            var statemachine = _factory.GetOrCreateStatemachine(job.Id);
            await statemachine.TransitAsync();
        }

        private async ValueTask SetStageTimeoutAsync(
            JobStage stage, 
            bool transitState = false,
            CancellationToken cancellationToken = default)
        {
            await _db.JobStages
                .Where(x => x.Id == stage.Id)
                .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                .SetField(x => x.Status).WithValue(PipelineJobStatus.Failed)
                .UpdateAsync();

            await _db.JobSteps
                .Where(x => x.PipelineJobStageId == stage.Id)
                .Where(x => !x.StartedAt.HasValue)
                .SetField(x => x.StartedAt).WithValue(DateTime.UtcNow)
                .UpdateAsync();

            await _db.JobSteps
                .Where(x => x.PipelineJobStageId == stage.Id)
                .Where(x => x.Status <= PipelineJobStatus.Running)
                .SetField(x => x.FinishedAt).WithValue(DateTime.UtcNow)
                .SetField(x => x.Status).WithValue(PipelineJobStatus.Failed)
                .UpdateAsync();

            if (stage.Steps != null)
            {
                foreach (var x in stage.Steps)
                {
                    await _log.PutLogAsync(
                        stage.PipelineJob.PipelineId,
                        stage.PipelineJob.Number,
                        "step-" + x.Id,
                        "The step has been cancelled due to the stage timeout",
                        LogLevel.Error,
                        cancellationToken);
                }
            }

            await _log.PutLogAsync(
                stage.PipelineJob.PipelineId,
                stage.PipelineJob.Number,
                "stage-" + stage.Id, 
                "Stage timeout", 
                LogLevel.Error, 
                cancellationToken);

            if (transitState)
            {
                var statemachine = _factory.GetOrCreateStatemachine(stage.PipelineJobId);
                await statemachine.TransitAsync();
            }
        }
    }

    public static class TimeKeeperExtensions
    {
        public static IServiceCollection AddTimeKeeper(this IServiceCollection self)
        {
            return self.AddSingleton<TimeKeeper>();
        }
    }
}
