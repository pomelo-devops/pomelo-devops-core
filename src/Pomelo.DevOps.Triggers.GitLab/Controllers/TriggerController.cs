// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.DevOps.Triggers.GitLab.Models;

namespace Pomelo.DevOps.Triggers.GitLab.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerController : ControllerBase
    {
        [HttpGet]
        public async ValueTask<IActionResult> Get(
            [FromServices] TriggerContext db,
            CancellationToken cancellationToken = default)
        {
            return Ok(await db.Triggers.ToListAsync(cancellationToken));
        }

        [HttpGet("{id:Guid}")]
        public async ValueTask<IActionResult> Get(
            [FromServices] TriggerContext db,
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.Triggers
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return NotFound("Trigger is not found");
            }

            return Ok(trigger);
        }

        [HttpPost]
        public async ValueTask<IActionResult> Post(
            [FromServices] TriggerContext db,
            [FromBody] Trigger request,
            CancellationToken cancellationToken = default)
        {
            db.Triggers.Add(request);
            await db.SaveChangesAsync(cancellationToken);
            return Ok(request);
        }

        [HttpDelete("{id:Guid}")]
        public async ValueTask<IActionResult> Delete(
            [FromServices] TriggerContext db,
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.Triggers
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return NotFound("Trigger is not found");
            }

            db.Triggers.Remove(trigger);
            await db.SaveChangesAsync(cancellationToken);
            return Ok("Trigger has been removed");
        }

        [HttpPatch("{id:Guid}")]
        public async ValueTask<IActionResult> Patch(
            [FromServices] TriggerContext db,
            [FromRoute] Guid id,
            [FromBody] Trigger request,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.Triggers
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return NotFound("Trigger is not found");
            }

            trigger.PomeloDevOpsProject = request.PomeloDevOpsProject;
            trigger.PomeloDevOpsPipeline = request.PomeloDevOpsPipeline;
            trigger.GitLabNamespace = request.GitLabNamespace;
            trigger.GitLabProject = request.GitLabProject;
            trigger.JobNameTemplate = request.JobNameTemplate;
            trigger.JobDescriptionTemplate = request.JobDescriptionTemplate;
            trigger.Enabled = request.Enabled;
            trigger.ArgumentsJson = request.ArgumentsJson;
            trigger.Name = request.Name;
            trigger.Branch = request.Branch;

            await db.SaveChangesAsync(cancellationToken);
            return Ok(trigger);
        }
    }
}
