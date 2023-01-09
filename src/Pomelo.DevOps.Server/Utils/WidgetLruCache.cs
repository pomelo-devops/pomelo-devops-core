// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LruCacheNet;
using Pomelo.DevOps.Models;

namespace Pomelo.DevOps.Server.Utils
{
    public class WidgetLruCache
    {
        private static LruCache<string, Widget> LruCache = new LruCache<string, Widget>(100);
        private PipelineContext db;

        public WidgetLruCache(PipelineContext db)
        {
            this.db = db;
        }

        public async ValueTask<Widget> GetWidgetAsync(
            string widgetId,
            CancellationToken cancellationToken = default)
        {
            Widget widget = null;
            if (!LruCache.ContainsKey(widgetId))
            {
                widget = await db.Widgets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == widgetId, cancellationToken);

                if (widget != null)
                {
                    LruCache.Add(widgetId, widget);
                }
            }
            else
            {
                widget = LruCache[widgetId];
            }

            return widget;
        }

        public void Retire(string widgetId)
        {
            LruCache.Remove(widgetId);
        }
    }

    public static class WidgetLruCacheExtensions
    {
        public static IServiceCollection AddWidgetLruCache(this IServiceCollection services)
            => services.AddScoped<WidgetLruCache>();
    }
}
