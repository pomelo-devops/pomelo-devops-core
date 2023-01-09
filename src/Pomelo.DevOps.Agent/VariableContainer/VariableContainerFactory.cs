// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;

namespace Pomelo.DevOps.Agent
{
    public class VariableContainerFactory
    {
        private ConcurrentDictionary<Guid, VariableContainer> _dic;
        private IServiceProvider _services;
        private ConfigManager _config;
        private Connector _connector;

        public VariableContainerFactory(IServiceProvider services)
        {
            _dic = new ConcurrentDictionary<Guid, Agent.VariableContainer>();
            _services = services;
            _connector = _services.GetRequiredService<Connector>();
            _config = _services.GetRequiredService<ConfigManager>();
        }

        public VariableContainer GetOrCreate(Guid stageId, string pipelineId, Guid jobId, long jobNumber)
        {
            return _dic.GetOrAdd(stageId, (id) =>
            {
                return new VariableContainer(_connector, _config, stageId, pipelineId, jobId, jobNumber);
            });
        }

        public bool IsExists(Guid id)
        {
            return _dic.ContainsKey(id);
        }

        public void Remove(Guid id)
        {
            _dic.TryRemove(id, out var test);
            test.Clear();
        }
    }

    public static class VariableContainerFactoryExtensions
    { 
        public static IServiceCollection AddVariableContainerFactory(this IServiceCollection self)
        {
            return self.AddSingleton<VariableContainerFactory>();
        }
    }
}
