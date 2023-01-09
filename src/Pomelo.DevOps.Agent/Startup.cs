// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.DevOps.Shared;
using Pomelo.DevOps.Models.ViewModels;
using Newtonsoft.Json.Converters;

namespace Pomelo.DevOps.Agent
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .AddControllersAsServices();

            services.AddConnector();
            services.AddGalleryManager();
            services.AddVariableContainerFactory();
            services.AddLogManager();
            services.AddMetricsLogger();
            services.AddStageStateMachineFactory();
            services.AddStageContainer();
            services.AddDispatcher();
            services.AddConfigManager();
            services.AddAdhocStepContainer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Pomelo DevOps Agent", Version = "v1", });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseErrorHandlingMiddleware();
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pomelo DevOps Agent");
            });
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
            });
            app.UseStaticFiles();
            var executor = app.ApplicationServices.GetRequiredService<Dispatcher>();
            executor.PollAsync(IsolationLevel.Parallel);
            executor.PollAsync(IsolationLevel.Sequential);
            var metrics = app.ApplicationServices.GetRequiredService<MetricsLogger>();
            metrics.StartWorkerAsync();
        }
    }
}
