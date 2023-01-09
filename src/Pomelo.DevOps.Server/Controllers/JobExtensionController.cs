// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Pomelo.DevOps.Shared;
using Microsoft.AspNetCore.Authorization;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobExtensionController : ControllerBase
    {
        [HttpGet]
        public async ValueTask<ApiResult<List<JobExtension>>> Get(
            [FromServices] PipelineContext db,
            CancellationToken cancellationToken = default)
        {
            var extensions = await db.JobExtensions
                .OrderBy(x => x.Priority)
                .ToListAsync(cancellationToken);

            if (!User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                extensions.ForEach(x => 
                {
                    x.Token = null; 
                });
            }

            return ApiResult(extensions);
        }

        [HttpGet("{id}")]

        public async ValueTask<ApiResult<JobExtension>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return ApiResult<JobExtension>(404, "Job extension is not found");
            }


            if (!User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                extension.Token = null;
            }

            return ApiResult(extension);
        }

        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpPut("{id}")]
        [HttpPatch("{id}")]
        public async ValueTask<ApiResult<JobExtension>> Patch(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromBody] PutJobExtensionRequest request,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return ApiResult<JobExtension>(404, "Job extension is not found");
            }

            extension.Token = request.Token;
            extension.ExtensionEntryUrl = request.ProviderEntryUrl;

            using var message = new HttpRequestMessage(HttpMethod.Get, request.ProviderEntryUrl);
            if (extension.Token != null)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Token", extension.Token);
            }
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var providerInfo = JsonConvert.DeserializeObject<JobExtensionInfo>(json);

            extension.Name = providerInfo.Name;
            extension.ViewUrl = providerInfo.View;
            extension.Description = providerInfo.Description;
            extension.IconUrl = providerInfo.Icon;
            extension.ManageUrl = providerInfo.Settings;

            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(extension);
        }

        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpPost]
        public async ValueTask<ApiResult<JobExtension>> Post(
            [FromServices] PipelineContext db,
            [FromBody] JobExtension request,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, request.ExtensionEntryUrl);
            if (request.Token != null)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Token", request.Token);
            }
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.GetAsync(request.ExtensionEntryUrl);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var providerInfo = JsonConvert.DeserializeObject<JobExtensionInfo>(json);
            request.Id = providerInfo.Id;
            request.Name = providerInfo.Name;
            request.ViewUrl = providerInfo.View;
            request.Description = providerInfo.Description;
            request.IconUrl = providerInfo.Icon;
            request.ManageUrl = providerInfo.Settings;
            db.JobExtensions.Add(request);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(request);
        }

        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpDelete("{id}")]
        public async ValueTask<ApiResult<JobExtension>> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return ApiResult<JobExtension>(404, "Job extension is not found");
            }

            db.JobExtensions.Remove(extension);
            await db.SaveChangesAsync(cancellationToken);

            return ApiResult(extension);
        }

        #region Proxy
        [HttpGet("{id}/{*endpoint}")]
        public async ValueTask<IActionResult> GetProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }

        [HttpPost("{id}/{*endpoint}")]
        public async ValueTask<ApiResult> PostProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return ApiResult(404, "Job extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return ApiResult((int)response.StatusCode, null);
        }

        [HttpPut("{id}/{*endpoint}")]
        public async ValueTask<IActionResult> PutProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }

        [HttpPatch("{id}/{*endpoint}")]
        public async ValueTask<IActionResult> PatchProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }

        [HttpDelete("{id}/{*endpoint}")]
        public async ValueTask<IActionResult> DeleteProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var extension = await db.JobExtensions
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (extension == null)
            {
                return NotFound("Job extension is not found");
            }

            using var message = Request.ToHttpRequestMessage(extension.ExtensionOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {extension.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }
        #endregion
    }
}
