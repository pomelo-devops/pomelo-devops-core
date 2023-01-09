// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Server.LogManager
{
    public class DbLogManager : ILogManager
    {
        private PipelineContext _db;

        public DbLogManager(PipelineContext db)
        {
            _db = db;
        }

        public async ValueTask<IEnumerable<Models.ViewModels.Log>> GetLogsAsync(string pipelineId, long jobNumber, string logSet, CancellationToken cancellationToken = default)
        {
            var query = _db.Logs
                .Where(x => x.PipelineId == pipelineId && x.JobNumber == jobNumber && x.LogSet == logSet)
                .OrderBy(x => x.Time);
            var result = await query.ToListAsync(cancellationToken);
            return result.Select(x => new Models.ViewModels.Log
            {
                Level = x.Level,
                Text = x.Text,
                Time = x.Time
            }).ToList();
        }

        public async ValueTask PutLogsAsync(string pipelineId, long jobNumber, string logSet, IEnumerable<Models.ViewModels.Log> logs, CancellationToken cancellationToken = default)
        {
            _db.Logs.AddRange(logs.Select(x => new Models.Log 
            {
                Level = x.Level,
                LogSet = logSet,
                PipelineId = pipelineId,
                JobNumber = jobNumber,
                Text = x.Text,
                Time = x.Time
            }));
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public static class DbLogManagerExtensions
    {
        public static IServiceCollection AddBuiltInLogManager(this IServiceCollection collection)
        {
            return collection.AddScoped<ILogManager, DbLogManager>();
        }
    }
}
