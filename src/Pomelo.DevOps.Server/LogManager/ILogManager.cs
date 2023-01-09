// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;

namespace Pomelo.DevOps.Server.LogManager
{
    public interface ILogManager
    {
        ValueTask<IEnumerable<Models.ViewModels.Log>> GetLogsAsync(string pipelineId, long jobNumber, string logSet, CancellationToken cancellationToken = default);
        ValueTask PutLogsAsync(string pipelineId, long jobNumber, string logSet, IEnumerable<Models.ViewModels.Log> logs, CancellationToken cancellationToken = default);
    }
}
