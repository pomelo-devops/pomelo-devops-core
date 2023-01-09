// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using Pomelo.DevOps.Server.Authentication;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Server.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Pomelo.DevOps.Server.LogManager;
using Pomelo.DevOps.Shared;
using Pomelo.DevOps.Server.MetricsManager;
using System.Diagnostics.CodeAnalysis;
using Pomelo.Vue.Middleware;
using Pomelo.DevOps.Server.UserManager;

namespace Pomelo.DevOps.Server
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "PomeloDevOps",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                })
                .AddControllersAsServices();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            services.AddAuthentication(x => x.DefaultScheme = TokenAuthenticateHandler.Scheme)
                .AddPersonalAccessToken();
            if (Configuration["Database:Type"].ToLower() == "mysql")
            {
                services.AddDbContext<PipelineContext>(x =>
                {
                    x.EnableServiceProviderCaching(false);
                    x.UseMySql(
                        Configuration["Database:ConnectionString"], 
                        ServerVersion.AutoDetect(Configuration["Database:ConnectionString"]), 
                        options => 
                        {
                            options.UseNewtonsoftJson();
                        });
                    x.UseMySqlLolita();
                });
            }
            else if (Configuration["Database:Type"].ToLower() == "sqlite")
            {
                services.AddDbContext<PipelineContext>(x =>
                {
                    x.EnableServiceProviderCaching(false);
                    x.UseSqlite(Configuration["Database:ConnectionString"]);
                    x.UseSqliteLolita();
                });
            }
            else
            {
                throw new NotSupportedException(Configuration["Database:Type"]);
            }

            if (string.IsNullOrEmpty(Configuration["Database:ScaleRedis"]))
            {
                services.AddSignalR();
            }
            else
            {
                services.AddSignalR().AddStackExchangeRedis(Configuration["Database:ScaleRedis"]);
            }
            services.AddTokenGenerator();
            services.AddJobStateMachineFactory();
            services.AddBuiltInLogManager();
            services.AddDbMetricsManager();
            services.AddSampleDataHelper();
            services.AddHttpContextAccessor();
            services.AddTimeKeeper();
            services.AddDbUserManager();
            services.AddWidgetLruCache();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Pomelo DevOps", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("PomeloDevOps");
            app.UseErrorHandlingMiddleware();
            app.UseForwardedHeaders();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pomelo DevOps");
            });
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
                endpoints.MapHub<Hubs.PipelineHub>("/api/pipelinehub");
            });
            app.UsePomeloVueMiddleware(x =>
            {
                x.AssetsVersion = "20221231";
                x.MappingPomeloVueJs = false;
                x.MappingBase = "/assets/js/pomelo-vue/";
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var sampleDataHelper = scope.ServiceProvider.GetRequiredService<SampleDataHelper>();
                sampleDataHelper.InitializeAsync().GetAwaiter().GetResult();
            }

            if (Convert.ToBoolean(Configuration["Services:TimeKeeper"]))
            {
                var timeKeeper = app.ApplicationServices.GetRequiredService<TimeKeeper>();
                timeKeeper.StartMonitorAsync();
            }
        }
    }
}
