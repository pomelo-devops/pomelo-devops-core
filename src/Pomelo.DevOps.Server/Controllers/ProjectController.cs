// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pomelo.DevOps.Models;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pomelo.DevOps.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        /// <summary>
        /// Get all of the projects. 
        /// All projects would be listed if the current user is system admin. 
        /// Otherwise, only the projects which the user has the access would be listed.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async ValueTask<ApiResult<List<Project>>> Get(
            [FromServices] PipelineContext db,
            CancellationToken cancellationToken = default)
        {
            if (User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                return ApiResult(await db.Projects.ToListAsync(cancellationToken));
            }
            else
            {
                return ApiResult(await db.Projects
                    .Where(x => x.Members.Any(y => y.UserId == Guid.Parse(User.Identity.Name)))
                    .ToListAsync(cancellationToken));
            }
        }

        /// <summary>
        /// Get single project with member list
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{projectId}")]
        public async ValueTask<ApiResult<Project>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default)
        {
            var project = await db.Projects
                .SingleOrDefaultAsync(x => x.Id == projectId, cancellationToken);

            if (project == null)
            {
                return ApiResult<Project>(404, $"The project has not been found");
            }

            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return ApiResult<Project>(403, "You have not authorized to access this project");
            }

            if (project.Dashboard == null)
            {
                project.Dashboard = new Dashboard();
            }

            return ApiResult(project);
        }

        /// <summary>
        /// Update or insert an project
        /// </summary>
        /// <param name="db"></param>
        /// <param name="body">Project body</param>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{projectId}")]
        [HttpPost("{projectId}")]
        [HttpPatch("{projectId}")]
        public async ValueTask<ApiResult<Project>> Put(
            [FromServices] PipelineContext db,
            [FromBody] PutProjectRequest body,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default)
        {
            if (!CommonIdRegex.IsMatch(projectId))
            {
                return ApiResult<Project>(400, "Invalid project Id");
            }

            if (!Convert.ToBoolean(await GetPolicyValueAsync(db, Policy.AllowUserCreateProject, cancellationToken))
                && !User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                return ApiResult<Project>(400, "System admin doesn't allow you to create project.");
            }

            var project = await db.Projects
                .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);

            if (body.IconBase64String != null)
            {
                var binary = GenerateBinary(Convert.FromBase64String(body.IconBase64String), projectId + ".png", "image/png");
                db.Binaries.Add(binary);
                body.IconId = binary.Id;
            }

            if (project != null) // Project already existed
            {
                if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
                {
                    return ApiResult<Project>(403, $"You don't have permission to patch this project.");
                }

                if (body.Name != null)
                {
                    project.Name = body.Name;
                }
                if (body.Dashboard != null)
                {
                    project.Dashboard = body.Dashboard;
                }
                if (body.Members.Count != 0)
                {
                    return ApiResult<Project>(400, $"Invalid operation. You could not patch members by this API.");
                }
            }
            else
            {
                project = body;
                project.Id = projectId;
                project.Members.Add(new ProjectMember 
                {
                    UserId = Guid.Parse(User.Identity.Name),
                    ProjectId = projectId,
                    Role = ProjectMemberRole.Admin
                });
                db.Projects.Add(project);
            }

            project.IconId = body.IconId;

            await db.SaveChangesAsync();
            return ApiResult((Project)body);
        }

        /// <summary>
        /// Delete an project
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{projectId}")]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId, 
            CancellationToken cancellationToken = default)
        {
            var project = await db.Projects.SingleOrDefaultAsync(x => x.Id == projectId, cancellationToken);
            if (project == null)
            {
                return ApiResult(400, $"The project id '{projectId}' is not found.");
            }

            db.Projects.Remove(project);
            await db.SaveChangesAsync();
            return ApiResult(200, $"The project '{project}' has been removed successfully.");
        }

        #region Members
        [HttpGet("{projectId}/member")]
        public async ValueTask<PagedApiResult<ProjectMember>> GetMembers(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromQuery] string name = null,
            [FromQuery] ProjectMemberRole? role = null,
            [FromQuery] int p = 1,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return PagedApiResult<ProjectMember>(403, "You don't have permission to list members in this project.");
            }

            IQueryable<ProjectMember> members = db.ProjectMembers
                .Include(x => x.User)
                .Where(x => x.ProjectId == projectId);

            if (!string.IsNullOrEmpty(name))
            {
                members = members.Where(x => x.User.DisplayName.Contains(name) || x.User.Username.Contains(name));
            }

            if (role.HasValue)
            {
                members = members.Where(x => x.Role == role);
            }

            return await PagedApiResultAsync(members, p, 20, cancellationToken);
        }

        [HttpGet("{projectId}/member/my")]
        public async ValueTask<ApiResult<ProjectMember>> GetMyMembership(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            CancellationToken cancellationToken = default) 
        {
            var userId = Guid.Parse(User.Identity.Name);
            return await GetMember(db, projectId, userId, cancellationToken);
        }

        [HttpGet("{projectId}/member/{userId:Guid}")]
        public async ValueTask<ApiResult<ProjectMember>> GetMember(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Member, cancellationToken))
            {
                return ApiResult<ProjectMember>(403, "You don't have permission to list members in this project.");
            }

            var member = await db.ProjectMembers
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId, cancellationToken);

            if (member == null)
            {
                return ApiResult<ProjectMember>(404, "Not found");
            }

            return ApiResult(member);
        }

        [HttpPost("{projectId}/member")]
        public async ValueTask<ApiResult<ProjectMember>> PostMember(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromBody] PostProjectMemberRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult<ProjectMember>(403, "You don't have permission to add members into this project.");
            }

            if (await db.ProjectMembers.AnyAsync(x => x.UserId == request.UserId && x.ProjectId == projectId, cancellationToken))
            {
                return ApiResult<ProjectMember>(400, "The user is already in this project.");
            }

            var projectMember = new ProjectMember
            {
                ProjectId = projectId,
                UserId = request.UserId,
                Role = request.Role
            };
            db.ProjectMembers.Add(projectMember);

            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(projectMember);
        }

        [HttpPut("{projectId}/member/{userId:Guid}")]
        public async ValueTask<ApiResult<ProjectMember>> PutMember(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] Guid userId,
            [FromBody] PostProjectMemberRequest request,
            CancellationToken cancellationToken = default)
        {
            request.UserId = userId;
            return await PostMember(db, projectId, request, cancellationToken);
        }

        [HttpPatch("{projectId}/member/{userId:Guid}")]
        public async ValueTask<ApiResult> PatchMember(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] Guid userId,
            [FromBody] PatchProjectMemberRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult(403, "You don't have permission to modify project member.");
            }

            await db.ProjectMembers
                .Where(x => x.UserId == userId && x.ProjectId == projectId)
                .SetField(x => x.Role).WithValue(request.Role)
                .UpdateAsync(cancellationToken);

            return ApiResult(200, "The specified member has been updated.");
        }

        [HttpDelete("{projectId}/member/{userId:Guid}")]
        public async ValueTask<ApiResult> DeleteMember(
            [FromServices] PipelineContext db,
            [FromRoute] string projectId,
            [FromRoute] Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (!await HasPermissionToProjectAsync(db, projectId, ProjectMemberRole.Admin, cancellationToken))
            {
                return ApiResult(403, "You don't have permission remove project member.");
            }

            await db.ProjectMembers
                .Where(x => x.UserId == userId && x.ProjectId == projectId)
                .DeleteAsync(cancellationToken);

            return ApiResult(200, "The specified member has been deleted.");
        }
        #endregion
    }
}
