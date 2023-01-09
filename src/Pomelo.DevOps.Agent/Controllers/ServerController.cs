// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Pomelo.DevOps.Agent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        [HttpPost("get")]
        public async ValueTask<IActionResult> Get(
            [FromServices] Connector connector,
            [FromBody] ForwardRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await connector.GetApiPlainAsync(request.Endpoint, cancellationToken);
            Response.StatusCode = result.StatusCode;
            return Content(result.Content, "application/json");
        }

        [HttpPost("post")]
        public async ValueTask<IActionResult> Post(
            [FromServices] Connector connector,
            [FromBody] ForwardRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await connector.PostApiPlainAsync(
                request.Endpoint, 
                JsonConvert.SerializeObject(request.Body), 
                cancellationToken);
            Response.StatusCode = result.StatusCode;
            return Content(result.Content, "application/json");
        }


        [HttpPost("delete")]
        public async ValueTask<IActionResult> Delete (
            [FromServices] Connector connector,
            [FromBody] ForwardRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await connector.DeleteApiPlainAsync(
                request.Endpoint,
                cancellationToken);
            Response.StatusCode = result.StatusCode;
            return Content(result.Content, "application/json");
        }
    }
}
