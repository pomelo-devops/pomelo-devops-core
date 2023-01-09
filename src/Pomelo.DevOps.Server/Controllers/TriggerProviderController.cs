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

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggerProviderController : ControllerBase
    {
        [HttpGet]
        public async ValueTask<ApiResult<List<TriggerProvider>>> Get(
            [FromServices] PipelineContext db,
            CancellationToken cancellationToken = default) 
        {
            var triggers = await db.TriggerProviders
                .OrderBy(x => x.Priority)
                .ToListAsync(cancellationToken);

            return ApiResult(triggers);
        }

        [HttpGet("{id}")]

        public async ValueTask<ApiResult<TriggerProvider>> Get(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.TriggerProviders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return ApiResult<TriggerProvider>(404, "Trigger provider is not found");
            }

            return ApiResult(trigger);
        }

        [HttpPut("{id}")]
        [HttpPatch("{id}")]
        public async ValueTask<ApiResult<TriggerProvider>> Patch(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromBody] PutTriggerProviderRequest request,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.TriggerProviders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return ApiResult<TriggerProvider>(404, "Trigger provider is not found");
            }

            trigger.Token = request.Token;
            trigger.ProviderEntryUrl = request.ProviderEntryUrl;

            using var message = new HttpRequestMessage(HttpMethod.Get, request.ProviderEntryUrl);
            if (trigger.Token != null)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Token", trigger.Token);
            }
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var providerInfo = JsonConvert.DeserializeObject<TriggerProviderInfo>(json);

            trigger.Name = providerInfo.Name;
            trigger.ViewUrl = providerInfo.View;
            trigger.Description = providerInfo.Description;
            trigger.IconUrl = providerInfo.Icon;
            trigger.ManageUrl = providerInfo.Settings;

            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(trigger);
        }

        [HttpPost]
        public async ValueTask<ApiResult<TriggerProvider>> Post(
            [FromServices] PipelineContext db,
            [FromBody] TriggerProvider request,
            CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, request.ProviderEntryUrl);
            if (request.Token != null)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Token", request.Token);
            }
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.GetAsync(request.ProviderEntryUrl);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var providerInfo = JsonConvert.DeserializeObject<TriggerProviderInfo>(json);
            request.Id = providerInfo.Id;
            request.Name = providerInfo.Name;
            request.ViewUrl = providerInfo.View;
            request.Description = providerInfo.Description;
            request.IconUrl = providerInfo.Icon;
            request.ManageUrl = providerInfo.Settings;
            db.TriggerProviders.Add(request);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(request);
        }

        [HttpDelete("{id}")]
        public async ValueTask<ApiResult<TriggerProvider>> Delete(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.TriggerProviders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return ApiResult<TriggerProvider>(404, "Trigger provider is not found");
            }

            db.TriggerProviders.Remove(trigger);
            await db.SaveChangesAsync(cancellationToken);

            return ApiResult(trigger);
        }

        #region Proxy
        [HttpGet("{id}/{*endpoint}")]
        public async ValueTask<IActionResult> GetProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.TriggerProviders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return NotFound("Trigger provider is not found");
            }

            using var message = Request.ToHttpRequestMessage(trigger.ProviderOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {trigger.Token}"
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
        public async ValueTask<IActionResult> PostProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.TriggerProviders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return NotFound("Trigger provider is not found");
            }

            using var message = Request.ToHttpRequestMessage(trigger.ProviderOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {trigger.Token}"
            });
            using var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 10)
            };
            using var response = await client.SendAsync(message, cancellationToken);
            await Response.ConsumeHttpResponseMessageAsync(response);
            return new EmptyResult();
        }

        [HttpPut("{id}/{*endpoint}")]
        public async ValueTask<IActionResult> PutProxy(
            [FromServices] PipelineContext db,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var trigger = await db.TriggerProviders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return NotFound("Trigger provider is not found");
            }

            using var message = Request.ToHttpRequestMessage(trigger.ProviderOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {trigger.Token}"
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
            var trigger = await db.TriggerProviders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return NotFound("Trigger provider is not found");
            }

            using var message = Request.ToHttpRequestMessage(trigger.ProviderOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {trigger.Token}"
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
            var trigger = await db.TriggerProviders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (trigger == null)
            {
                return NotFound("Trigger provider is not found");
            }

            using var message = Request.ToHttpRequestMessage(trigger.ProviderOriginalBaseUrl + "/" + endpoint, new Dictionary<string, string>
            {
                ["Authorization"] = $"Token {trigger.Token}"
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
