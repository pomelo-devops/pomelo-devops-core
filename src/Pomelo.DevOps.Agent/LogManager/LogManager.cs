// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Agent
{
    public class LogManager
    {
        private Connector _connector;

        public LogManager(Connector connector)
        {
            _connector = connector;
        }

        public async ValueTask LogAsync(JobStage job, string logSet, string log, LogLevel level, CancellationToken cancellationToken = default)
        {
            try 
            {
                await _connector.LogAsync(job, logSet, new List<Pomelo.DevOps.Models.ViewModels.Log> { new Pomelo.DevOps.Models.ViewModels.Log 
                {
                    Level = level,
                    Text = log,
                    Time = DateTime.UtcNow
                } }, cancellationToken);
            }
            catch (ConnectorException)
            { }
        }
    }

    public static class LogManagerExtensions
    {
        public static IServiceCollection AddLogManager(this IServiceCollection collection)
        {
            return collection.AddSingleton<LogManager>();
        }
    }
}
