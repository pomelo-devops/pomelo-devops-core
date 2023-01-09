// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.DevOps.Server.Utils
{
    public static class ControllerExtensions
    {
        public static IPEndPoint GetRemoteIPAddress(this ControllerBase controller)
        {
            IPEndPoint ip;
            var headers = controller.HttpContext.Request.Headers.ToList();
            if (headers.Exists((kvp) => kvp.Key == "X-Forwarded-For"))
            {
                var header = headers.First((kvp) => kvp.Key == "X-Forwarded-For").Value.ToString();
                ip = IPEndPoint.Parse(header);
            }
            else
            {
                ip = new IPEndPoint(controller.HttpContext.Connection.RemoteIpAddress, controller.HttpContext.Connection.RemotePort);
            }
            return ip;
        }
    }
}
