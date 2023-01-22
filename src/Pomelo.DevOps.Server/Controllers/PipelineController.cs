// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.Workflow;
using Pomelo.Workflow.Models;
using Pomelo.Workflow.Models.ViewModels;

namespace Pomelo.DevOps.Server.Controllers
{
    [Authorize]
    [Route("api/project/{projectId}/[controller]")]
    [ApiController]
    public class PipelineController : ControllerBase
    {
        /// <summary>
        /// Get pipelines from project
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async ValueTask<ApiResult<List<Models.Pipeline>>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default)
        {
            if (await db.Projects.CountAsync(x => x.Id == projectId, cancellationToken) == 0)
            {
                return ApiResult<List<Models.Pipeline>>(404, $"The project '{projectId}' has not been found");
            }

            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return ApiResult<List<Models.Pipeline>>(401, $"You don't have the permission to project '{projectId}'");
            }

            var ret = await db.Pipelines
                .Include(x => x.User)
                .Where(x => x.ProjectId == projectId)
                .Where(x => x.Visibility == PipelineVisibility.Public 
                    || x.UserId == Guid.Parse(User.Identity.Name)
                    || x.Accesses.Any(y => y.PipelineId == x.Id && y.UserId == Guid.Parse(User.Identity.Name)))
                .OrderBy(x => x.Visibility)
                .ToListAsync(cancellationToken);

            foreach(var pipeline in ret)
            {
                pipeline.Jobs = await db.Jobs
                    .Where(x => x.PipelineId == pipeline.Id)
                    .OrderByDescending(x => x.TriggeredAt)
                    .Take(50)
                    .ToListAsync(cancellationToken);
            }

            return ApiResult(ret);
        }

        [HttpGet("fav")]
        public async ValueTask<ApiResult<List<string>>> Fav(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            CancellationToken token = default)
        {
            var fav = await db.PipelineFavorites
                .Where(x => x.UserId == Guid.Parse(User.Identity.Name))
                .Select(x => x.PipelineId)
                .ToListAsync(token);

            return ApiResult(fav);
        }

        /// <summary>
        /// Get pipeline definiations
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pipeline"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{pipeline}")]
        public async ValueTask<ApiResult<GetPipelineResponse>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string pipeline,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default)
        {
            if (await db.Pipelines.CountAsync(x => x.Id == pipeline && x.ProjectId == projectId, cancellationToken) == 0)
            {
                return ApiResult<GetPipelineResponse>(404, $"The pipeline '{pipeline}' has not been found");
            }

            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<GetPipelineResponse>(401, $"You don't have the permission to this pipeline");
            }

            var _pipeline = await db.Pipelines
                .Where(x => x.Id == pipeline
                    && x.ProjectId == projectId)
                .SingleAsync(cancellationToken);

            var pipelineDefinition = YamlDeserializer
                .Deserialize<GetPipelineResponse>(_pipeline.PipelineBody);

            GetPipelineResponse ret = pipelineDefinition with 
            {
                Id = _pipeline.Id,
                Name = _pipeline.Name,
                ProjectId = _pipeline.ProjectId,
                UserId = _pipeline.UserId,
                Visibility = _pipeline.Visibility
            };

            ret.Stages = ret.Stages.Select(x =>
            {
                x.Steps = x.Steps.OrderBy(x => x.Order).ToList();
                return x;
            }).ToList();

            return ApiResult(ret);
        }

        /// <summary>
        /// Get pipeline definiations in YAML
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pipeline"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{pipeline}.yml")]
        public async ValueTask<string> GetYaml(
            [FromServices] PipelineContext db,
            [FromRoute] string pipeline,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default)
        {
            var result = await Get(db, pipeline, projectId, cancellationToken);
            if (result.Code != 200)
            {
                Response.StatusCode = result.Code;
                Response.ContentType = "text/plain";
                return result.Message;
            }
            else
            {
                Response.ContentType = "application/x-yaml";
                return YamlSerializer.Serialize(result.Data);
            }
        }

        [HttpGet("{pipeline}/constant")]
        public async ValueTask<ApiResult<List<PipelineConstants>>> GetConstant(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<List<PipelineConstants>>(401, $"You don't have the permission to this pipeline");
            }

            var items = (await Get(db, pipeline, projectId, cancellationToken)).Data.Constants;

            return ApiResult(items);
        }

        [HttpPut("{pipeline}")]
        [HttpPost("{pipeline}")]
        [HttpPatch("{pipeline}")]
        public async ValueTask<ApiResult<Pipeline>> Put(
            [FromServices] PipelineContext db,
            [FromServices] WorkflowManager wf,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromBody] PutPipelineRequest request,
            [FromQuery] bool isYaml = false,
            CancellationToken cancellationToken = default)
        {
            if (!CommonIdRegex.IsMatch(pipeline))
            {
                return ApiResult<Pipeline>(400, "The pipeline ID is invalid.");
            }

            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult<Pipeline>(403, $"You don't have the permission to project '{projectId}'");
            }

            var _pipeline = await db.Pipelines
                .FirstOrDefaultAsync(x => x.Id == pipeline, cancellationToken);

            if (_pipeline == null)
            {
                _pipeline = new Pipeline();
                _pipeline.Id = pipeline;
                _pipeline.UserId = Guid.Parse(User.Identity.Name);
                _pipeline.ProjectId = projectId;
                _pipeline.Type = request.Type;

                if (request.Type == PipelineType.Diagram)
                {
                    var workflow = await wf.CreateWorkflowAsync(
                        new CreateWorkflowRequest { Name = "", Description = "" }, 
                        true, 
                        cancellationToken);
                    _pipeline.WorkflowId = workflow;
                }

                db.Pipelines.Add(_pipeline);
            }
            else
            {
                if (_pipeline.Type != request.Type)
                {
                    return ApiResult<Pipeline>(400, "You cannot change pipeline type.");
                }
            }

            if (!isYaml)
            {
                _pipeline.Name = request.Name;
                _pipeline.Visibility = request.Visibility;
            }

            if (_pipeline.Type == PipelineType.Linear)
            {
                // Checks
                if (request.Stages.Count > 0)
                {
                    var minOrder = request.Stages.Min(x => x.Order);
                    var maxOrder = request.Stages.Max(x => x.Order);
                    if (minOrder != 1)
                    {
                        return ApiResult<Pipeline>(400, "Stage min order must be 1");
                    }

                    for (var i = 1; i < maxOrder; ++i)
                    {
                        if (!request.Stages.Any(x => x.Order == i))
                        {
                            return ApiResult<Pipeline>(400, "Stage order missing: " + i);
                        }
                    }

                    var emptyStages = request.Stages
                        .Where(x => x.Steps.Count == 0)
                        .ToList();

                    if (emptyStages.Count > 0)
                    {
                        return ApiResult<Pipeline>(400, $"Missing step in those stage(s): {string.Join(", ", emptyStages.Select(x => x.Name))}");
                    }
                }

                _pipeline.PipelineBody = YamlSerializer.Serialize(request);
            }
            

            await db.SaveChangesAsync();
            return ApiResult(_pipeline);
        }

        [HttpPut("{pipeline}.yml")]
        public async ValueTask<ApiResult<Pipeline>> Put(
            [FromServices] PipelineContext db,
            [FromServices] WorkflowManager wf,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromQuery] string name,
            [FromQuery] PipelineVisibility? visibility,
            CancellationToken cancellationToken = default)
        {
            if (!CommonIdRegex.IsMatch(pipeline))
            {
                return ApiResult<Pipeline>(400, "The pipeline ID is invalid.");
            }

            using (var sr = new StreamReader(Request.Body))
            {
                var yaml = sr.ReadToEnd();
                var _pipeline = YamlDeserializer.Deserialize<PutPipelineRequest>(yaml);
                _pipeline.Name = name;
                if (visibility.HasValue)
                {
                    _pipeline.Visibility = visibility.Value;
                }
                return await Put(db, wf, projectId, pipeline, _pipeline, false, cancellationToken);
            }
        }

        [HttpDelete("{pipeline}")]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult(403, $"You don't have the permission to project '{projectId}'");
            }

            await db.PipelineTriggers
                .Where(x => x.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            await db.PipelineAccesses
                .Where(x => x.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            await db.PipelineFavorites
                .Where(x => x.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            await db.JobSteps
                .Where(x => x.PipelineJobStage.PipelineJob.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            await db.JobStages
                .Where(x => x.PipelineJob.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            await db.JobVariables
                .Where(x => x.PipelineJob.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            await db.JobLabels
                .Where(x => x.Job.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            await db.Jobs
                .Where(x => x.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            await db.Pipelines
                .Where(x => x.Id == pipeline)
                .DeleteAsync(cancellationToken);

            return ApiResult(200, $"The specified pipeline '{pipeline}' has been removed successfully");
        }

        [HttpGet("{pipeline}/labels")]
        public async ValueTask<ApiResult<List<string>>> GetLabels(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<List<string>>(403, $"You don't have the permission to this pipeline");
            }

            var labels = await db.JobLabels
                .Where(x => x.Job.PipelineId == pipeline)
                .GroupBy(x => x.Label)
                .Select(x => x.Key)
                .ToListAsync(cancellationToken);

            return ApiResult(labels);
        }

        #region Diagram Stages
        [HttpGet("{pipeline}/diagram-stage")]
        public async ValueTask<ApiResult<List<PipelineDiagramStage>>> GetDiagramStages(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromQuery] string name = null,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<List<PipelineDiagramStage>>(403, $"You don't have the permission to this pipeline");
            }

            IQueryable<PipelineDiagramStage> stages = db.PipelineDiagramStages;

            if (!string.IsNullOrEmpty(name))
            {
                stages = stages.Where(x => x.Workflow.Name.Contains(name));
            }

            return ApiResult(await stages.ToListAsync(cancellationToken));
        }

        [HttpGet("{pipeline}/diagram-stage/{diagramStageId:Guid}")]
        public async ValueTask<ApiResult<PipelineDiagramStage>> GetDiagramStage(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid diagramStageId,
            [FromQuery] string name = null,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<PipelineDiagramStage>(403, $"You don't have the permission to this pipeline");
            }

            var stage = await db.PipelineDiagramStages
                .Include(x => x.Workflow)
                .FirstOrDefaultAsync(x => x.PipelineId == pipeline 
                    && x.Id == diagramStageId, cancellationToken);

            if (stage == null)
            {
                return ApiResult<PipelineDiagramStage>(404, $"The specified diagram stage was not found.");
            }

            return ApiResult(stage);
        }

        [HttpDelete("{pipeline}/diagram-stage/{diagramStageId:Guid}")]
        public async ValueTask<ApiResult<PipelineDiagramStage>> DeleteDiagramStage(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid diagramStageId,
            [FromQuery] string name = null,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult<PipelineDiagramStage>(403, $"You don't have the permission to this pipeline");
            }

            var stage = await db.PipelineDiagramStages
                .Include(x => x.Workflow)
                .FirstOrDefaultAsync(x => x.PipelineId == pipeline
                    && x.Id == diagramStageId, cancellationToken);

            if (stage == null)
            {
                return ApiResult<PipelineDiagramStage>(404, $"The specified diagram stage was not found.");
            }

            return ApiResult(stage);
        }

        [HttpPost("{pipeline}/diagram-stage")]
        public async ValueTask<ApiResult<PipelineDiagramStage>> PostDiagramStage(
            [FromServices] PipelineContext db,
            [FromServices] WorkflowManager wf,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromBody] CreateWorkflowRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult<PipelineDiagramStage>(403, $"You don't have the permission to this pipeline");
            }

            var workflow = await wf.CreateWorkflowAsync(new CreateWorkflowRequest
            {
                Description = request.Description,
                Name = request.Name
            }, true, cancellationToken);

            var pipelineDiagramStage = new PipelineDiagramStage
            {
                PipelineId = pipeline,
                WorkflowId = workflow
            };
            db.PipelineDiagramStages.Add(pipelineDiagramStage);

            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(pipelineDiagramStage);
        }
        #endregion

        #region Pipeline Access
        [HttpGet("{pipeline}/access")]
        public async ValueTask<PagedApiResult<PipelineAccess>> GetAccesses(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromQuery] string name = null,
            [FromQuery] PipelineAccessType? type = null,
            [FromQuery] int p = 1,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return PagedApiResult<PipelineAccess>(403, "You don't have permission to list pipeline accesses");
            }

            IQueryable<PipelineAccess> accesses = db.PipelineAccesses
                .Include(x => x.User)
                .Where(x => x.PipelineId == pipeline);

            if (!string.IsNullOrEmpty(name))
            {
                accesses = accesses.Where(x => x.User.DisplayName.Contains(name) || x.User.Username.Contains(name));
            }

            if (type.HasValue)
            {
                accesses = accesses.Where(x => x.AccessType == type);
            }

            return await PagedApiResultAsync(accesses, p, 20, cancellationToken);
        }

        [HttpGet("{pipeline}/access/my")]
        public async ValueTask<ApiResult<PipelineAccess>> GetMyAccess(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(User.Identity.Name);
            return await GetAccess(db, projectId, pipeline, userId, cancellationToken);
        }

        [HttpGet("{pipeline}/access/{userId:Guid}")]
        public async ValueTask<ApiResult<PipelineAccess>> GetAccess(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return ApiResult<PipelineAccess>(403, "You don't have permission to view pipeline access.");
            }

            var access = await db.PipelineAccesses
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.PipelineId == pipeline && x.UserId == userId, cancellationToken);

            if (access == null)
            {
                return ApiResult<PipelineAccess>(404, "Not found");
            }

            return ApiResult(access);
        }

        [HttpPost("{pipeline}/access")]
        public async ValueTask<ApiResult<PipelineAccess>> PostAccess(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromBody] PostPipelineAccessRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult<PipelineAccess>(403, "You don't have permission to add pipeline access.");
            }

            if (await db.PipelineAccesses.AnyAsync(x => x.UserId == request.UserId && x.PipelineId == pipeline, cancellationToken))
            {
                return ApiResult<PipelineAccess>(400, "The user already has access to this pipeline");
            }

            var pipelineAccess = new PipelineAccess
            {
                PipelineId = pipeline,
                UserId = request.UserId,
                AccessType = request.AccessType
            };
            db.PipelineAccesses.Add(pipelineAccess);

            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(pipelineAccess);
        }

        [HttpPut("{pipeline}/access/{userId:Guid}")]
        public async ValueTask<ApiResult<PipelineAccess>> PutAccess(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid userId,
            [FromBody] PostPipelineAccessRequest request,
            CancellationToken cancellationToken = default)
        {
            request.UserId = userId;
            return await PostAccess(db, projectId, pipeline, request, cancellationToken);
        }

        [HttpPatch("{pipeline}/access/{userId:Guid}")]
        public async ValueTask<ApiResult> PatchAccess(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid userId,
            [FromBody] PatchPipelineAccessRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult(403, "You don't have permission to modify pipeline access.");
            }

            await db.PipelineAccesses
                .Where(x => x.UserId == userId && x.PipelineId == pipeline)
                .SetField(x => x.AccessType).WithValue(request.AccessType)
                .UpdateAsync(cancellationToken);

            return ApiResult(200, "The specified member has been updated.");
        }

        [HttpDelete("{pipeline}/access/{userId:Guid}")]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult(403, "You don't have permission to delete pipeline access");
            }

            await db.PipelineAccesses
                .Where(x => x.UserId == userId && x.PipelineId == pipeline)
                .DeleteAsync(cancellationToken);

            return ApiResult(200, "The specified access has been deleted.");
        }
        #endregion

        #region Triggers
        [HttpGet("{pipeline}/trigger")]
        public async ValueTask<ApiResult<List<PipelineTrigger>>> GetTriggers(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<List<PipelineTrigger>>(403, "You don't have permission to get triggers of this pipeline.");
            }

            var triggers = await db.PipelineTriggers
                .Where(x => x.PipelineId == pipeline)
                .ToListAsync(cancellationToken);

            return ApiResult(triggers);
        }

        [HttpGet("{pipeline}/trigger/{triggerId:Guid}")]
        public async ValueTask<ApiResult<PipelineTrigger>> GetTrigger(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid triggerId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Reader, cancellationToken))
            {
                return ApiResult<PipelineTrigger>(403, "You don't have permission to get triggers of this pipeline.");
            }

            var trigger = await db.PipelineTriggers
                .Where(x => x.PipelineId == pipeline && x.Id == triggerId)
                .FirstOrDefaultAsync(cancellationToken);

            return ApiResult(trigger);
        }

        [HttpDelete("{pipeline}/trigger/{triggerId:Guid}")]
        public async ValueTask<ApiResult> DeleteTrigger(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid triggerId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult(403, "You don't have permission to modify triggers on this pipeline.");
            }

            await db.PipelineTriggers
                .Where(x => x.PipelineId == pipeline && x.Id == triggerId)
                .DeleteAsync(cancellationToken);

            return ApiResult(200, "The trigger has been removed");
        }

        [HttpPatch("{pipeline}/trigger/{triggerId:Guid}")]
        public async ValueTask<ApiResult> PatchTrigger(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromRoute] Guid triggerId,
            [FromBody] PipelineTrigger request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult(403, "You don't have permission to modify triggers on this pipeline.");
            }

            await db.PipelineTriggers
                .Where(x => x.PipelineId == pipeline && x.Id == triggerId)
                .SetField(x => x.Enabled).WithValue(request.Enabled)
                .SetField(x => x.Name).WithValue(request.Name)
                .UpdateAsync(cancellationToken);

            return ApiResult(200, "The trigger has been updated");
        }

        [HttpPost("{pipeline}/trigger")]
        public async ValueTask<ApiResult<PipelineTrigger>> PostTrigger(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] string pipeline,
            [FromBody] PipelineTrigger request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToPipelineAsync(db, projectId, pipeline, PipelineAccessType.Master, cancellationToken))
            {
                return ApiResult<PipelineTrigger>(403, "You don't have permission to create triggers on this pipeline.");
            }

            request.PipelineId = pipeline;
            db.PipelineTriggers.Add(request);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(request);
        }
        #endregion
    }
}
