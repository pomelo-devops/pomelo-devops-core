// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.DevOps.Agent
{
    public static class ConnectorExtensions
    {
        public static IServiceCollection AddConnector(this IServiceCollection self)
        {
            return self.AddSingleton<Connector>();
        }
    }
}
