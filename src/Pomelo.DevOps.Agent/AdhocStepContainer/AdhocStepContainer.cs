// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Agent.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Pomelo.DevOps.Agent
{
    public class AdhocStepContainer
    {
        private ConcurrentDictionary<Guid, AdhocStep> _dic;

        public AdhocStepContainer()
        {
            _dic = new ConcurrentDictionary<Guid, AdhocStep>();
        }

        public void Put(Guid id, AdhocStep step)
        {
            _dic[id] = step;
        }

        public AdhocStep Get(Guid id)
        {
            if (!_dic.ContainsKey(id))
            {
                return null;
            }
            return _dic[id];
        }

        public void Remove(Guid id)
        {
            _dic.Remove(id, out var item);
        }
    }

    public static class AdhocStepContainerExtensions
    {
        public static IServiceCollection AddAdhocStepContainer(this IServiceCollection collection)
        {
            return collection.AddSingleton<AdhocStepContainer>();
        }
    }
}
