// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Server.Utils;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WidgetController : ControllerBase
    {
        internal static FileExtensionContentTypeProvider FileExtensionContentTypeProvider 
            = new FileExtensionContentTypeProvider();

        [HttpGet]
        public async ValueTask<PagedApiResult<WidgetBase>> Get(
            [FromServices] PipelineContext db,
            [FromQuery] string name = null,
            [FromQuery] int p = 1,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Widget> query = db.Widgets;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name)
                    || x.Id.Contains(name));
            }

            return await PagedApiResultAsync(
                query.Select(x => new WidgetBase 
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IconUrl = x.IconUrl,
                    ConfigEntryView = x.ConfigEntryView,
                    WidgetEntryView = x.WidgetEntryView
                }), 
                p, 
                20, 
                cancellationToken);
        }

        [HttpGet("{id}")]
        public async ValueTask<ApiResult<Widget>> Get(
            [FromServices] WidgetLruCache widgetLruCache,
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            var widget = await widgetLruCache.GetWidgetAsync(id, cancellationToken);

            if (widget == null)
            {
                return ApiResult<Widget>(404, "The specified widget was not found");
            }

            return ApiResult(widget);
        }

        [HttpGet("{id}/export")]
        public async ValueTask<IActionResult> GetJson(
            [FromServices] WidgetLruCache widgetLruCache,
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            var widget = await widgetLruCache.GetWidgetAsync(id, cancellationToken);

            if (widget == null)
            {
                return NotFound("The specified widget was not found");
            }

            return File(
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(widget)), 
                "application/octet-stream", 
                id + ".json");
        }

        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpPut]
        [HttpPost]
        [HttpPatch]
        public async ValueTask<ApiResult<Widget>> Post(
            [FromServices] PipelineContext db,
            [FromServices] WidgetLruCache widgetLruCache,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (file.Length > 1024 * 1024 * 4)
            {
                return ApiResult<Widget>(400, "The widget size is too large.");
            }

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var jsonText = await reader.ReadToEndAsync();
            var widget = JsonConvert.DeserializeObject<Widget>(jsonText);
            return await Post(db, widgetLruCache, widget.Id, widget, cancellationToken);
        }

        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        [HttpPut("{id}")]
        [HttpPost("{id}")]
        [HttpPatch("{id}")]
        public async ValueTask<ApiResult<Widget>> Post(
            [FromServices] PipelineContext db,
            [FromServices] WidgetLruCache widgetLruCache,
            [FromRoute] string id,
            [FromBody] Widget request,
            CancellationToken cancellationToken = default)
        {
            if (Request.ContentLength > 1024 * 1024 * 4)
            {
                return ApiResult<Widget>(400, "The widget size is too large.");
            }

            request.Id = id;

            var widget = await db.Widgets
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (widget == null)
            {
                widget = request;
                db.Widgets.Add(widget);
            }
            else
            {
                widget.Name = request.Name;
                widget.Description = request.Description;
                widget.IconUrl = request.IconUrl;
                widget.WidgetEntryView = request.WidgetEntryView;
                widget.ConfigEntryView = request.ConfigEntryView;
                widget.Items = request.Items;
            }
            widgetLruCache.Retire(id);
            await db.SaveChangesAsync(cancellationToken);
            return ApiResult(widget);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.SystemAdmin))]
        public async ValueTask<ApiResult> Delete(
            [FromServices] PipelineContext db,
            [FromServices] WidgetLruCache widgetLruCache,
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            await db.Widgets
                .Where(x => x.Id == id)
                .DeleteAsync(cancellationToken);

            widgetLruCache.Retire(id);

            return ApiResult(200, "The specified widget has been removed");
        }

        #region Proxy
        [AllowAnonymous]
        [HttpGet("{id}/resource/{*endpoint}")]
        public async ValueTask<IActionResult> Get(
            [FromServices] WidgetLruCache widgetLruCache,
            [FromRoute] string id,
            [FromRoute] string endpoint,
            CancellationToken cancellationToken = default)
        {
            var widget = await widgetLruCache.GetWidgetAsync(id, cancellationToken);

            if (widget == null)
            {
                return NotFound("The specified widget was not found");
            }

            if (widget.Items == null)
            {
                return NotFound("The specified resource was not found");
            }

            var item = FindItemInItems(widget.Items, endpoint);
            if (item == null)
            {
                return NotFound("The specified resource was not found");
            }

            if (!FileExtensionContentTypeProvider.TryGetContentType(item.Name, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(item.Content, contentType, item.Name);
        }

        private WidgetPackageItem FindItemInItems(IEnumerable<WidgetPackageItem> searchBase, string endpoint)
        {
            var splited = endpoint.Split('/');
            if (splited.Length == 1)
            {
                return searchBase.FirstOrDefault(x => x.Name == splited[0]);
            }

            var folder = searchBase.FirstOrDefault(x => x.Name == splited[0] 
                && x.Type == WidgetPackageItemType.Directory);

            if (folder == null)
            {
                return null;
            }

            return FindItemInItems(folder.Children, string.Join('/', splited.Skip(1)));
        }
        #endregion
    }
}
