// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Pomelo.DevOps.Server.Controllers;
using Pomelo.DevOps.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Server.Utils
{
    public class SampleDataHelper
    {
        private PipelineContext _db;

        public SampleDataHelper(PipelineContext db)
        {
            _db = db;
        }

        public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
        {
            await _db.Database.MigrateAsync(cancellationToken);
            if (await _db.Projects.CountAsync(cancellationToken) == 0)
            {
                await _db.InstallSampleDataAsync(cancellationToken);
                var path = Directory.GetCurrentDirectory();
                var packagesPath = Path.Combine(Directory.GetParent(Directory.GetParent(path).FullName).FullName, "steps", "bin");
                if (!Directory.Exists(packagesPath))
                {
                    return;
                }
                var packages = Directory.EnumerateFiles(packagesPath, "*.pdo");
                var adminUserId = (await _db.Users.FirstAsync()).Id;
                foreach (var package in packages)
                {
                    var fileInfo = new FileInfo(package);
                    using (var stream = new MemoryStream(File.ReadAllBytes(package)))
                    using (var pdo = new PomeloDevOpsPackage(stream))
                    {
                        var packageBytes = new byte[fileInfo.Length];
                        var step = await pdo.DeserializeYamlAsync();
                        var existed = await _db.GalleryPackages
                            .Where(x => x.Id == step.Id)
                            .ToListAsync(cancellationToken);

                        Binary icon = null;
                        try
                        {
                            var iconBytes = await pdo.GetComponentIconAsync();
                            icon = ControllerBase.GenerateBinary(iconBytes, $"{step.Id}-{step.Version}.png", "application/x-png");
                        }
                        catch (FileNotFoundException)
                        {
                            // Use default icon for the package
                        }

                        stream.Position = 0;
                        var binary = await ControllerBase.GenerateBinaryAsync(stream, $"{step.Id}-{step.Version}.pdo", "application/zip", (int)fileInfo.Length);

                        Platform platform = default;
                        if (step.Methods.All(x => x.Entry.Windows != null))
                        {
                            platform |= Platform.Windows;
                        }
                        if (step.Methods.All(x => x.Entry.Linux != null))
                        {
                            platform |= Platform.Linux;
                        }
                        if (step.Methods.All(x => x.Entry.Mac != null))
                        {
                            platform |= Platform.Mac;
                        }

                        var _package = new GalleryPackage
                        {
                            Id = step.Id,
                            Version = step.Version,
                            AuthorId = adminUserId,
                            Icon = icon,
                            ListInGallery = true,
                            Description = step.Description,
                            Name = step.Name,
                            Package = binary,
                            Platform = platform,
                            Methods = step.Methods == null ? null : string.Join(',', step.Methods.Select(x => x.Name))
                        };
                        _db.GalleryPackages.Add(_package);
                        await _db.SaveChangesAsync();
                    } 
                }
            }
        }
    }

    public static class SampleDataHelperExtensions
    { 
        public static IServiceCollection AddSampleDataHelper(this IServiceCollection collection)
        {
            return collection.AddScoped<SampleDataHelper>();
        }
    }      
}
