// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Server.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.DevOps.Server.UserManager;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static readonly Random random = new Random();

        [HttpGet]
        [Authorize]
        public async ValueTask<PagedApiResult<User>> Get(
            [FromServices] PipelineContext db,
            [FromQuery] UserRole? role,
            [FromQuery] string name = null,
            [FromQuery] string provider = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] int p = 1,
            CancellationToken cancellationToken = default)
        {
            if (!User.IsInRole(nameof(UserRole.SystemAdmin))
                && !await db.ProjectMembers.AnyAsync(x => x.UserId == Guid.Parse(User.Identity.Name) 
                    && x.Role == ProjectMemberRole.Admin))
            {
                return PagedApiResult<User>(403, "You don't have permission to list users.");
            }

            IQueryable<User> query = db.Users;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.DisplayName.Contains(name)
                    || x.Username.Contains(name));
            }

            if (role.HasValue)
            {
                query = query.Where(x => x.Role == role.Value);
            }

            if (!string.IsNullOrEmpty(provider))
            {
                query = query.Where(x => x.LoginProviderId == provider);
            }

            if (userId.HasValue)
            {
                query = query.Where(x => x.Id == userId.Value);
            }

            return await PagedApiResultAsync(query, p, 20, cancellationToken);
        }

        /// <summary>
        /// Get a user information
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user">User ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{providerId}/{user}")]
        public async ValueTask<ApiResult<User>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string providerId,
            [FromRoute] string user,
            CancellationToken cancellationToken = default)
        {
            var _user = await db.Users
                .SingleOrDefaultAsync(x => x.Username == user && x.LoginProviderId == providerId, cancellationToken);

            if (_user == null)
            {
                return ApiResult<User>(404, "User not found");
            }

            return ApiResult(_user);
        }

        [HttpPut("{providerId}/{user}")]
        [HttpPost("{providerId}/{user}")]
        public async ValueTask<ApiResult<User>> Put(
            [FromServices] PipelineContext db,
            [FromRoute] string user,
            [FromBody] PutUserRequest request,
            CancellationToken cancellationToken = default)
        {
            var provider = await db.LoginProviders
                .FirstOrDefaultAsync(x => x.Mode == LoginProviderType.Local, cancellationToken);

            if (provider == null)
            {
                return ApiResult<User>(400, "Your administrator doesn't allow register.");
            }

            if (!provider.Enabled && !User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                return ApiResult<User>(400, "Your administrator doesn't allow register.");
            }

            if (await db.Users.AnyAsync(x => x.LoginProviderId == provider.Id && x.Username == user, cancellationToken))
            {
                return ApiResult<User>(400, "Username was already taken.");
            }

            var salt = new byte[64];
            random.NextBytes(salt);
            var hash = Crypto.ComputeSha256Hash(Encoding.ASCII.GetBytes(request.Password), salt);
            var _user = new User
            {
                Salt = salt,
                Email = request.Email,
                DisplayName = user,
                PasswordHash = hash,
                Role = UserRole.Collaborator,
                LoginProviderId = provider.Id,
                Username = user
            };
            db.Users.Add(_user);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(_user);
        }

        [Authorize]
        [HttpPatch("{providerId}/{user}")]
        public async ValueTask<ApiResult<User>> Patch(
            [FromServices] PipelineContext db,
            [FromRoute] string providerId,
            [FromRoute] string user,
            [FromBody] PatchUserRequest request,
            CancellationToken cancellationToken = default)
        {
            var _user = await db.Users
                .FirstOrDefaultAsync(x => x.Username == user && x.LoginProviderId == providerId, cancellationToken);

            if (_user == null)
            {
                return ApiResult<User>(404, "User was not found");
            }

            if (_user.Id != Guid.Parse(User.Identity.Name) && !User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                return ApiResult<User>(403, "No permission");
            }

            var provider = await db.LoginProviders
                .FirstOrDefaultAsync(x => x.Id == providerId);

            if (provider.Mode == LoginProviderType.Local)
            {
                _user.Email = request.Email;
                _user.DisplayName = request.DisplayName;

                if (!string.IsNullOrEmpty(request.RawPassword))
                {
                    _user.Salt = new byte[64];
                    random.NextBytes(_user.Salt);
                    _user.PasswordHash = Crypto.ComputeSha256Hash(Encoding.ASCII.GetBytes(request.RawPassword), _user.Salt);
                }
            }

            if (User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                _user.Role = request.Role;
            }
            _user.AvatarId = request.AvatarId;
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(_user);
        }


        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpDelete("{providerId}/{user}")]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string providerId,
            [FromRoute] string user,
            CancellationToken cancellationToken = default)
        {
            var _user = await db.Users
                .FirstOrDefaultAsync(x => x.Username == user && x.LoginProviderId == providerId, cancellationToken);

            if (_user == null)
            {
                return ApiResult(404, "User was not found");
            }

            if (_user.Id == Guid.Parse(User.Identity.Name))
            {
                return ApiResult(400, "You can't remove yourself");
            }

            db.Users.Remove(_user);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(200, "User has been removed");
        }

        /// <summary>
        /// Get the session status, includes requested time and expire time
        /// </summary>
        /// <param name="db"></param>
        /// <param name="token">Token</param>
        /// <param name="user">User ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{providerId}/{user}/session")]
        public async ValueTask<ApiResult<UserSession>> GetSession(
        [FromServices] PipelineContext db,
        [FromQuery] string session,
        [FromRoute] string user,
        [FromRoute] string providerId,
        CancellationToken cancellationToken = default)
        {
            var _session = await db.UserSessions
                .Where(x => x.ExpireAt > DateTime.UtcNow
                    && x.Id == session
                    && x.User.Username == user
                    && x.User.LoginProviderId == providerId)
                .SingleOrDefaultAsync(cancellationToken);

            if (_session == null)
            {
                return ApiResult<UserSession>(404, "The session is not found.");
            }

            return ApiResult(_session);
        }

        /// <summary>
        /// Request a session token to request APIs
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user">User ID</param>
        /// <param name="body">Request body</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{providerId}/{user}/session")]
        [HttpPost("{providerId}/{user}/session")]
        [HttpPatch("{providerId}/{user}/session")]
        public async ValueTask<ApiResult<UserSession>> PostSession(
            [FromServices] PipelineContext db,
            [FromServices] ITokenGenerator tokenGenerator,
            [FromServices] DbUserManager userManager,
            [FromRoute] string user,
            [FromBody] PostSessionRequest body,
            CancellationToken cancellationToken = default)
        {
            if (!await userManager.ValidateUserAsync(user, body.Password, cancellationToken))
            {
                return ApiResult<UserSession>(401, "Unauthorized");
            }

            while (true)
            {
                // If the token conflicted, we need to retry
                // TODO: Collect expired tokens
                UserSession session = null;
                try
                {
                    var _user = await db.Users
                        .FirstAsync(x => x.Username == user && x.LoginProvider.Mode == LoginProviderType.Local, cancellationToken);

                    var bytes = tokenGenerator.Generate(32);
                    session = new UserSession
                    {
                        Id = Convert.ToBase64String(bytes),
                        Address = HttpContext.Connection.RemoteIpAddress.ToString(),
                        Agent = HttpContext.Request.Headers["User-Agent"].ToString(),
                        UserId = _user.Id
                    };
                    db.UserSessions.Add(session);
                    await db.SaveChangesAsync(cancellationToken);
                    session.User = _user;
                    return ApiResult(session);
                }
                catch (Exception ex)
                {
                    if (session != null)
                    {
                        db.UserSessions.Remove(session);
                    }

                    var exceptionTypes = new Type[] { typeof(DbUpdateException), typeof(InvalidOperationException), typeof(ArgumentException) };
                    if (!exceptionTypes.Contains(ex.GetType()))
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Delete the session of user
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user">User ID</param>
        /// <param name="token">Session token in base64 string format</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("{providerId}/{user}/session")]
        public async ValueTask<ApiResult> DeleteSession(
            [FromServices] PipelineContext db,
            [FromRoute] string providerId,
            [FromRoute] string user,
            [FromQuery] string session,
            CancellationToken cancellationToken = default)
        {
            var _session = await db.UserSessions
                .Where(x => x.ExpireAt > DateTime.UtcNow
                    && x.Id == session
                    && x.User.Username == user
                    && x.User.LoginProviderId == providerId)
                .SingleOrDefaultAsync(cancellationToken);

            if (_session == null)
            {
                return ApiResult(404, "The session is not found.");
            }

            if (!User.IsInRole(nameof(UserRole.SystemAdmin)) && Guid.Parse(User.Identity.Name) != _session.UserId)
            {
                return ApiResult(401, "You don't have the permission to this session");
            }

            db.UserSessions.Remove(_session);
            await db.SaveChangesAsync();
            return ApiResult(200, "The session has been removed successfully");
        }

        [Authorize]
        [HttpGet("{user}/pat")]
        public async ValueTask<ApiResult<List<PersonalAccessToken>>> GetPat(
            [FromServices] PipelineContext db,
            [FromRoute] Guid user,
            CancellationToken cancellationToken = default)
        {
            if (user != Guid.Parse(User.Identity.Name))
            {
                return ApiResult<List<PersonalAccessToken>>(403, "You don't have permission to list PAT of this user.");
            }

            return ApiResult(await db.PersonalAccessTokens
                .Where(x => x.UserId == user)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new PersonalAccessToken
                {
                    CreatedAt = x.CreatedAt,
                    ExpireAt = x.ExpireAt,
                    Id = x.Id,
                    Name = x.Name,
                    UserId = x.UserId,
                })
                .ToListAsync(cancellationToken));
        }

        [HttpPost("{user}/pat")]
        public async ValueTask<ApiResult<PersonalAccessToken>> PostPat(
            [FromServices] PipelineContext db,
            [FromRoute] Guid user,
            [FromBody] PersonalAccessToken accessToken,
            CancellationToken cancellationToken = default)
        {
            if (user != Guid.Parse(User.Identity.Name))
            {
                return ApiResult<PersonalAccessToken>(403, "You don't have permission to create PAT for this user.");
            }

            accessToken.CreatedAt = DateTime.UtcNow;
            accessToken.UserId = user;
            accessToken.Token = "pdo_" + RandomGenerator.Generate(60);

            db.PersonalAccessTokens.Add(accessToken);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(accessToken);
        }

        [HttpDelete("{user}/pat/{patId:Guid}")]
        public async ValueTask<ApiResult> DeletePat(
            [FromServices] PipelineContext db,
            [FromRoute] Guid user,
            [FromRoute] Guid patId,
            CancellationToken cancellationToken = default)
        {
            if (user != Guid.Parse(User.Identity.Name))
            {
                return ApiResult(403, "You don't have permission to delete PAT for this user.");
            }

            await db.PersonalAccessTokens
                .Where(x => x.UserId == user)
                .Where(x => x.Id == patId)
                .DeleteAsync(cancellationToken);

            return ApiResult(200, "The PAT has been deleted.");
        }

        [Authorize]
        [HttpGet("{providerId}/{user}/project-access")]
        public async ValueTask<ApiResult<List<ProjectMember>>> GetProjectAccess(
            [FromServices] PipelineContext db,
            [FromRoute] string providerId,
            [FromRoute] string user,
            CancellationToken cancellationToken = default)
        {
            var _user = await db.Users
                .FirstOrDefaultAsync(x => x.Username == user 
                    && x.LoginProviderId == providerId, cancellationToken);

            if (_user == null)
            {
                return ApiResult<List<ProjectMember>>(404, "The specified user was not found");
            }

            var accesses = await db.ProjectMembers
                .Where(x => x.UserId == _user.Id)
                .ToListAsync(cancellationToken);

            return ApiResult(accesses);
        }
    }
}
