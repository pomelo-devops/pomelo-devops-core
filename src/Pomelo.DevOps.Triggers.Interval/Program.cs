// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Pomelo.DevOps.Triggers.Interval.Authentication;
using Pomelo.DevOps.Triggers.Interval.Models;
using Pomelo.DevOps.Shared;
using Pomelo.Vue.Middleware;
using Pomelo.DevOps.Triggers.Interval;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
        options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    })
    .AddControllersAsServices();

if (builder.Configuration["Database:Type"].ToLower() == "mysql")
{
    builder.Services.AddDbContext<TriggerContext>(x =>
    {
        x.UseMySql(builder.Configuration["Database:ConnectionString"], ServerVersion.AutoDetect(builder.Configuration["Database:ConnectionString"]));
    });
}
else if (builder.Configuration["Database:Type"].ToLower() == "sqlite")
{
    throw new NotSupportedException("This trigger provider doesn't support SQLite");
}
else
{
    throw new NotSupportedException(builder.Configuration["Database:Type"]);
}

builder.Services.AddAuthentication(x => x.DefaultScheme = TokenAuthenticateHandler.Scheme)
    .AddToken();
builder.Services.AddSingleton<Interval>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseErrorHandlingMiddleware();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
});
app.UsePomeloVueMiddleware(x =>
{
    x.AssetsVersion = "v20221206-1";
    x.MappingPomeloVueJs = false;
    x.MappingBase = "/assets/js/pomelo-vue/";
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TriggerContext>();
    await db.Database.MigrateAsync();
}
app.Services.GetRequiredService<Interval>().Start();
app.Run();
