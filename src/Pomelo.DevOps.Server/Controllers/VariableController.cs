// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/project/{projectId}/pipeline/{pipeline}/job/{jobNumber}/[controller]")]
    [ApiController]
    [Authorize]
    public class VariableController : ControllerBase
    {
        /// <summary>
        /// Get the variables from a pipeline job
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pipeline">Pipeline ID</param>
        /// <param name="job">Job ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async ValueTask<ApiResult<List<JobVariable>>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            CancellationToken cancellationToken = default) 
        { 
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<List<JobVariable>>(401, "You don't have permission to access this pipeline");
            }

            var _job = await db.Jobs
                .Include(x => x.Variables)
                .SingleOrDefaultAsync(x => x.PipelineId == pipeline && x.Number == jobNumber, cancellationToken);

            if (_job == null)
            {
                return ApiResult<List<JobVariable>>(404, "The pipeline job has not been found");
            }

            return ApiResult(_job.Variables.ToList());
        }

        /// <summary>
        /// Get specified variable from a pipeline job
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="pipeline">Pipeline ID</param>
        /// <param name="job">Job ID</param>
        /// <param name="variable">Variable Name</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{variable}")]
        public async ValueTask<ApiResult<JobVariable>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromRoute] string variable,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<JobVariable>(401, "You don't have permission to access this pipeline");
            }

            var _variable = await db.JobVariables
                .SingleOrDefaultAsync(x => x.Name == variable 
                    && x.PipelineJob.PipelineId == pipeline 
                    && x.PipelineJob.Number == jobNumber, cancellationToken);

            if (_variable == null)
            {
                return ApiResult<JobVariable>(404, "The pipeline job variable has not been found");
            }

            return ApiResult(_variable);
        }

        /// <summary>
        /// Create or update a pipeline job variable
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId"></param>
        /// <param name="pipeline"></param>
        /// <param name="job"></param>
        /// <param name="variable"></param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{variable}")]
        [HttpPost("{variable}")]
        [HttpPatch("{variable}")]
        public async ValueTask<ApiResult<JobVariable>> Put(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromRoute] string variable,
            [FromBody] JobVariable body,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult<JobVariable>(401, "You don't have permission to access this pipeline");
            }

            var _variable = await db.JobVariables
                .SingleOrDefaultAsync(x => x.Name == variable 
                    && x.PipelineJob.PipelineId == pipeline
                    && x.PipelineJob.Number == jobNumber, cancellationToken);

            if (_variable == null)
            {
                body.PipelineJobId = await JobController.GetJobIdByNumberAsync(db, pipeline, jobNumber);
                body.Name = variable;
                db.JobVariables.Add(body);
                await db.SaveChangesAsync();
                return ApiResult(body);
            }
            else
            {
                _variable.Value = body.Value;
                await db.SaveChangesAsync();
                return ApiResult(_variable);
            }
        }

        /// <summary>
        /// Delete a pipeline job variable
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId"></param>
        /// <param name="pipeline"></param>
        /// <param name="job"></param>
        /// <param name="variable"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{variable}")]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] long jobNumber,
            [FromRoute] string variable,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Collaborator, cancellationToken))
            {
                return ApiResult(401, "You don't have permission to access this pipeline");
            }

            var _variable = await db.JobVariables
                .SingleOrDefaultAsync(x => x.Name == variable && x.PipelineJob.PipelineId == pipeline 
                    && x.PipelineJob.PipelineId == pipeline
                    && x.PipelineJob.Number == jobNumber, cancellationToken);

            if (_variable == null)
            {
                return ApiResult(404, "The pipeline job variable has not been found");
            }

            db.JobVariables.Remove(_variable);
            await db.SaveChangesAsync();

            return ApiResult(200, "Variables has been removed");
        }
    }
}
