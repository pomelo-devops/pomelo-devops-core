// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyController : ControllerBase
    {
        [HttpGet]
        public async ValueTask<ApiResult<List<Policy>>> Get(
            [FromServices] PipelineContext db,
            CancellationToken cancellationToken = default) 
        {
            var result = await db.Policies
                .OrderBy(x => x.Order)
                .ToListAsync(cancellationToken);

            result.ForEach(x => x.Value = JsonConvert.DeserializeObject(x.ValueJson));

            return ApiResult(result);
        }

        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpPut]
        [HttpPost]
        [HttpPatch]
        public async ValueTask<ApiResult> Patch(
            [FromServices] PipelineContext db,
            [FromRoute] string key,
            [FromBody] PatchPolicyRequest request,
            CancellationToken cancellationToken = default)
        {
            request.Policies.ForEach(x => x.ValueJson = JsonConvert.SerializeObject(x.Value));
            await db.Policies
                .DeleteAsync(cancellationToken);
            db.Policies.AddRange(request.Policies);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(200, "Policy updated");
        }
    }
}
