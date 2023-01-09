// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.LoginProviders;
using Pomelo.DevOps.Models.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginProviderController : ControllerBase
    {
        [HttpGet]
        public async ValueTask<ApiResult<List<object>>> Get(
            [FromServices] PipelineContext db,
            [FromQuery] bool includeInvisible = false,
            CancellationToken cancellationToken = default)
        {
            if (includeInvisible && !User.IsInRole("SystemAdmin"))
            {
                return ApiResult<List<object>>(403, "You don't have permission to list all login providers.");
            }

            IQueryable<LoginProvider> query = db.LoginProviders;
            if (!includeInvisible)
            {
                query = query.Where(x => x.Enabled);
            }

            var providers = await query
                .OrderByDescending(x => x.Priority)
                .ToListAsync(cancellationToken);

            var ret = new List<object>(providers.Count);
            foreach (var provider in providers)
            {
                var type = LoginProviderUtils.GetLoginTypeByMode(provider.Mode);
                if (provider.Body == null)
                {
                    ret.Add(provider);
                    continue;
                }

                var obj = JsonConvert.DeserializeObject(provider.Body, type) as LoginProvider;
                var parsedProvider = obj with
                {
                    Id = provider.Id,
                    IconUrl = provider.IconUrl,
                    Enabled = provider.Enabled,
                    Mode = provider.Mode,
                    Name = provider.Name,
                    Priority = provider.Priority,
                    ClientSecret = provider.ClientSecret
                };

                if (!User.IsInRole("SystemAdmin"))
                {
                    parsedProvider.ClientSecret = null;
                }

                ret.Add(parsedProvider);
            }

            return ApiResult(ret);
        }

        [HttpGet("{providerId}")]
        public async ValueTask<ApiResult<object>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string providerId,
            CancellationToken cancellationToken = default)
        {
            var provider = await db.LoginProviders
                .FirstOrDefaultAsync(x => x.Id == providerId, cancellationToken);

            if (provider == null)
            {
                return ApiResult(404, "The login provider was not found");
            }

            if (!provider.Enabled && !User.IsInRole("SystemAdmin"))
            {
                return ApiResult(403, "You don't have permission to query this provider");
            }


            var type = LoginProviderUtils.GetLoginTypeByMode(provider.Mode);
            var obj = JsonConvert.DeserializeObject(provider.Body, type) as LoginProvider;
            var parsedProvider = obj with
            {
                Id = provider.Id,
                IconUrl = provider.IconUrl,
                Enabled = provider.Enabled,
                Mode = provider.Mode,
                Name = provider.Name,
                Priority = provider.Priority,
                ClientSecret = provider.ClientSecret
            };

            if (!User.IsInRole("SystemAdmin"))
            {
                parsedProvider.ClientSecret = null;
            }

            return ApiResult(parsedProvider as object);
        }

        [HttpPut("{providerId}")]
        [HttpPost("{providerId}")]
        [HttpPatch("{providerId}")]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async ValueTask<ApiResult<object>> Post(
            [FromServices] PipelineContext db,
            [FromRoute] string providerId,
            CancellationToken cancellationToken = default)
        {
            using var streamReader = new StreamReader(Request.Body);
            var jsonString = await streamReader.ReadToEndAsync();
            var provider = JsonConvert.DeserializeObject<LoginProvider>(jsonString);
            var providerType = LoginProviderUtils.GetLoginTypeByMode(provider.Mode);

            var _provider = await db.LoginProviders
                .FirstOrDefaultAsync(x => x.Id == providerId, cancellationToken);

            if (_provider == null)
            {
                if (provider.Mode == LoginProviderType.Local && await db.LoginProviders.AnyAsync(x => x.Mode == LoginProviderType.Local, cancellationToken))
                {
                    return ApiResult<object>(400, "You can't add more local login provider");
                }

                _provider = new LoginProvider 
                {
                    Id = providerId,
                    Mode = provider.Mode
                };
                db.LoginProviders.Add(_provider);
            }

            if (jsonString != null) // Generate new body
            {
                var obj = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(jsonString, providerType)));
                // Remove base class properties
                foreach(var property in typeof(LoginProvider).GetProperties().Select(x => x.Name))
                {
                    obj.Remove(property);
                }

                _provider.Body = JsonConvert.SerializeObject(obj);
            }

            _provider.Enabled = provider.Enabled;
            _provider.Name = provider.Name;
            _provider.Priority = provider.Priority;
            _provider.IconUrl = provider.IconUrl;
            _provider.ClientSecret = provider.ClientSecret;
            _provider.Enabled = provider.Enabled;

            await db.SaveChangesAsync(cancellationToken);

            return await Get(db, _provider.Id, cancellationToken);
        }

        [HttpDelete("{providerId}")]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string providerId,
            CancellationToken cancellationToken = default)
        {
            var provider = await db.LoginProviders
                .FirstOrDefaultAsync(x => x.Id == providerId, cancellationToken);

            if (provider == null)
            {
                return ApiResult(404, "The specified login provider was not found");
            }

            db.LoginProviders.Remove(provider);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(200, "Login provider removed");
        }

        internal static T CombineProvider<T>(LoginProvider provider)
            where T : LoginProvider
        {
            var providerType = LoginProviderUtils.GetLoginTypeByMode(provider.Mode);

            var obj = JsonConvert.DeserializeObject<T>(provider.Body ?? "{}");
            var parsedProvider = obj with
            {
                Id = provider.Id,
                IconUrl = provider.IconUrl,
                Enabled = provider.Enabled,
                Mode = provider.Mode,
                Name = provider.Name,
                Priority = provider.Priority,
                ClientSecret = provider.ClientSecret
            };
            return parsedProvider;
        }
    }
}
