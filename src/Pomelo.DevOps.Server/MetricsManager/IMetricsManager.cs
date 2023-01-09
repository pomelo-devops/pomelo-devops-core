// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Server.MetricsManager
{
    public interface IMetricsManager
    {
        ValueTask<IEnumerable<Metrics>> GetAsync(Guid agent, MetricsType type, long begin, long end, CancellationToken cancellationToken = default);
        ValueTask PutAsync(Metrics metrics, CancellationToken cancellationToken = default);
    }
}
