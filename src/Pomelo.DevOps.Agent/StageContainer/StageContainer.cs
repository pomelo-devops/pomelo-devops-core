// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Pomelo.DevOps.Agent
{
    public class StageContainer
    {
        private ConcurrentDictionary<Guid, JobStage> _dic;

        public StageContainer()
        {
            _dic = new ConcurrentDictionary<Guid, JobStage>();
        }

        public void Add(JobStage stage)
        {
            _dic.AddOrUpdate(stage.Id, stage, (id, s) => {
                return stage;
            });
        }

        public IEnumerable<Guid> GetStageIds()
        {
            return _dic.Select(x => x.Key);
        }

        public JobStage Get(Guid id)
        {
            if (!_dic.ContainsKey(id))
            {
                return null;
            }

            return _dic[id];
        }

        public bool IsExists(Guid id)
        {
            return _dic.ContainsKey(id);
        }

        public void Remove(Guid id)
        {
            _dic.TryRemove(id, out var val);
        }

        public IEnumerable<JobStage> Stages => _dic.Values;
    }

    public static class StageContainerExtensions
    { 
        public static IServiceCollection AddStageContainer(this IServiceCollection self)
        {
            return self.AddSingleton<StageContainer>();
        }
    }
}
