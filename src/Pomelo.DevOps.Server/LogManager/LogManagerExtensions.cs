// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Server.LogManager
{
    public static class LogManagerExtensions
    {
        public static ValueTask PutLogAsync(this ILogManager self, string pipelineId, long jobNumber, string logSet, string text, LogLevel level, CancellationToken cancellationToken = default)
        {
            return self.PutLogsAsync(pipelineId, jobNumber, logSet, new List<Models.ViewModels.Log> { new Models.ViewModels.Log 
            {
                Level = level,
                Text = text,
                Time = DateTime.UtcNow
            } }, cancellationToken);
        }
    }
}
