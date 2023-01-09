// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Server.Hubs
{
    [ExcludeFromCodeCoverage]
    public class PipelineHub : Hub
    {
        public async ValueTask Join(string group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }

        public async ValueTask Quit(string group)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }
    }
}
