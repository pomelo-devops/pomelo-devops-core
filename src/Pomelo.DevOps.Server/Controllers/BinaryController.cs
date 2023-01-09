// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BinaryController : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(
            [FromServices] PipelineContext db,
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var bin = await db.Binaries
                .Include(x => x.Partitions)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (bin == null)
            {
                return NotFound();
            }

            var bytes = bin.Partitions
                .OrderBy(x => x.Partition)
                .SelectMany(x => x.Bytes)
                .ToArray();

            return File(bytes, bin.ContentType);
        }
    }
}
