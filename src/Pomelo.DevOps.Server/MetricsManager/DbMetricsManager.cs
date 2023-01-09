// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.DevOps.Server.MetricsManager
{
    public class DbMetricsManager : IMetricsManager
    {
        private PipelineContext _db;
        public DbMetricsManager(PipelineContext db)
        {
            _db = db;
        }

        public async ValueTask<IEnumerable<Metrics>> GetAsync(
            Guid agent,
            MetricsType type,
            long begin,
            long end,
            CancellationToken cancellationToken)
        {
            return await _db.Metrics
                .Where(x => x.Type == type)
                .Where(x => x.AgentId == agent)
                .Where(x => begin <= x.Timestamp)
                .Where(x => x.Timestamp <= end)
                .OrderBy(x => x.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async ValueTask PutAsync(Metrics metrics, CancellationToken cancellationToken)
        {
            _db.Metrics.Add(metrics);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public static class DbMetricsManagerExtensions
    {
        public static IServiceCollection AddDbMetricsManager(this IServiceCollection collection)
        {
            return collection.AddScoped<IMetricsManager, DbMetricsManager>();
        }
    }
}
