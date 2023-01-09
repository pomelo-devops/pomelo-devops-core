// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Agent
{
    public class MetricsLogger : IDisposable
    {
        private Connector _connector;
        private ConfigManager _config;
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _memoryCounter;
        private int _counter = 0;

        public MetricsLogger(Connector connector, ConfigManager config)
        {
            _connector = connector;
            _config = config;
        }

        public void StartMonitor()
        {
            Interlocked.Increment(ref _counter);
        }

        public void StopMonitor()
        {
            Interlocked.Decrement(ref _counter);
        }

        public async ValueTask StartWorkerAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var p = Process.Start("lodctr", "/r"))
                {
                    p.WaitForExit();
                }
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Process", "Working Set", "_Total");
            }

            while (true)
            {
                if (_counter == 0)
                {
                    await Task.Delay(1000);
                    continue;
                }

                if (string.IsNullOrEmpty(_config.Config.ProjectId))
                {
                    await Task.Delay(5000);
                    continue;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _connector.PostMetricsAsync(_config.Config.ProjectId, _config.Config.AgentPoolId, GenerateMetrics(MetricsType.Processor));
                    _connector.PostMetricsAsync(_config.Config.ProjectId, _config.Config.AgentPoolId, GenerateMetrics(MetricsType.Memory));
                }

                await Task.Delay(1000);
            }
        }

        private Metrics GenerateMetrics(MetricsType type)
        {
            if (!_config.Config.AgentId.HasValue)
            {
                return null;
            }

            try
            {
                if (type == MetricsType.Processor)
                {
                    var value = _cpuCounter.NextValue();
                    return new Metrics
                    {
                        AgentId = _config.Config.AgentId.Value,
                        Timestamp = DateTime.UtcNow.Ticks,
                        Type = type,
                        Value = value
                    };
                }
                else if (type == MetricsType.Memory)
                {
                    var value = Process.GetProcesses().AsParallel().Sum(x => x.WorkingSet64) / 1024.0 / 1024.0;
                    return new Metrics
                    {
                        AgentId = _config.Config.AgentId.Value,
                        Timestamp = DateTime.UtcNow.Ticks,
                        Type = type,
                        Value = value
                    };
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
        }
    }

    public static class MetricsLoggerExtensions
    {
        public static IServiceCollection AddMetricsLogger(this IServiceCollection collection)
        {
            return collection.AddSingleton<MetricsLogger>();
        }
    }
}
