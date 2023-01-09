// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Server.Utils;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Novell.Directory.Ldap;
using Pomelo.DevOps.Models.LoginProviders;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]/{providerId}")]
    [ApiController]
    public class LdapController : ControllerBase
    {
        private static HttpClient client = new HttpClient();

        [HttpPost]
        public async ValueTask<ApiResult<UserSession>> Post(
            [FromServices] PipelineContext db,
            [FromServices] ITokenGenerator tokenGenerator,
            [FromRoute] string providerId,
            [FromBody] PostLdapLoginRequest request,
            CancellationToken cancellationToken = default)
        {
            var _provider = await db.LoginProviders
                .FirstOrDefaultAsync(x => x.Id == providerId, cancellationToken);

            if (_provider == null)
            {
                return ApiResult<UserSession>(404, "Login provider not found");
            }

            var provider = LoginProviderController.CombineProvider<LdapLogin>(_provider);

            if (!provider.Enabled)
            {
                return ApiResult<UserSession>(404, "Login provider not enabled");
            }

            using var ldapConnection = new LdapConnection();
            await ldapConnection.ConnectAsync(provider.LdapServer, provider.Port);
            await ldapConnection.BindAsync(request.Username, request.Password);
            if (!ldapConnection.Bound)
            {
                return ApiResult<UserSession>(404, "Invalid Credential");
            }

            var attributes = new[] 
            {
                provider.UsernamePath, 
                provider.EmailPath,
                provider.DisplayNamePath 
            };

            var ldapResults = await ldapConnection.SearchAsync(
                "CN=Users,DC=cscad,DC=net",
                LdapConnection.ScopeSub,
                $"(&(ObjectCategory=user)({provider.UsernamePath}={request.Username}))",
                attributes,
                false);

            string username = null, displayName = null, email = null;
            await foreach (var item in ldapResults)
            {
                username = item.GetAttribute(provider.UsernamePath).StringValue;
                email = item.GetAttribute(provider.EmailPath).StringValue;
                displayName = item.GetAttribute(provider.DisplayNamePath).StringValue;
                break;
            }

            if (username == null)
            {
                return ApiResult<UserSession>(404, "Invalid Credential");
            }

            // Get or create user
            var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username && x.LoginProviderId == provider.Id);
            if (user == null)
            {
                user = new User
                {
                    Username = username,
                    Email = email,
                    Salt = new byte[0],
                    PasswordHash = new byte[0],
                    Role = UserRole.Collaborator,
                    DisplayName = displayName,
                    LoginProviderId = provider.Id
                };
                db.Users.Add(user);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Update user
                user.Email = email;
                displayName = user.DisplayName;
            }

            while (true)
            {
                // If the token conflicted, we need to retry
                // TODO: Collect expired tokens
                UserSession session = null;
                try
                {
                    var bytes = tokenGenerator.Generate(32);
                    session = new UserSession
                    {
                        Id = Convert.ToBase64String(bytes),
                        Address = HttpContext.Connection.RemoteIpAddress.ToString(),
                        Agent = HttpContext.Request.Headers["User-Agent"].ToString(),
                        UserId = user.Id
                    };
                    db.UserSessions.Add(session);
                    await db.SaveChangesAsync(cancellationToken);
                    session.User = user;
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
    }
}
