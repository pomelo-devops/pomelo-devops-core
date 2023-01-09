// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Shared;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Web;
using System;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GalleryController : ControllerBase
    {
        private static FileExtensionContentTypeProvider mimeProvider = new FileExtensionContentTypeProvider();

        private IConfiguration _config;
        protected IConfiguration Config
        {
            get
            {
                if (_config == null)
                {
                    _config = HttpContext.RequestServices?.GetService<IConfiguration>();
                }
                return _config;
            }
        }

        private static HttpClient _client = new HttpClient();

        [Authorize]
        [HttpPost("form")]
        public async ValueTask<IActionResult> PostForm(
            [FromServices] PipelineContext db,
            IFormFile package,
            CancellationToken cancellationToken = default)
        {
            var result = await Post(db, package, cancellationToken);
            if (result.Code == 200)
            {
                return Redirect($"/gallery/package/{result.Data.Id}/version/{result.Data.Version}");
            }
            else
            {
                return Redirect($"/gallery/upload/fail?error={HttpUtility.UrlPathEncode(result.Message)}");
            }
        }

        /// <summary>
        /// Get package list
        /// </summary>
        /// <param name="db"></param>
        /// <param name="platform">Platform</param>
        /// <param name="type">Controller step or Runner step</param>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async ValueTask<PagedApiResult<GalleryPackage>> Get(
            [FromServices] PipelineContext db,
            [FromQuery] string name = null,
            [FromQuery] Guid? author = null,
            [FromQuery] int p = 1,
            [FromQuery] Platform? platform = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<GalleryPackage> packages = db.GalleryPackages;

            if (!string.IsNullOrEmpty(name))
            {
                packages = packages.Where(x => x.Name.Contains(name));
            }

            if (author.HasValue)
            {
                packages = packages.Where(x => x.AuthorId == author);
            }

            if (platform.HasValue)
            {
                packages = packages.Where(x => x.Platform.HasFlag(platform));
            }

            packages = packages.OrderByDescending(x => x.Version);

            var totalRecords = await packages
                .GroupBy(x => x.Id)
                .Select(x => x.Key)
                .CountAsync(cancellationToken);

            var data = packages;
            var pageSize = 20;
            var currentPage = p;
            data = data
                .GroupBy(x => x.Id)
                .Select(x => x.OrderByDescending(x => x.Version).First())
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            return new PagedApiResult<GalleryPackage>
            {
                Code = 200,
                Message = "Succeeded",
                Data = await data.ToListAsync(cancellationToken),
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (totalRecords + pageSize - 1) / pageSize
            };
        }

        /// <summary>
        /// Get all versions of specified package
        /// </summary>
        /// <param name="db"></param>
        /// <param name="package">Package ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{package}")]
        public async ValueTask<ApiResult<List<GalleryPackage>>> GetPackage(
            [FromServices] PipelineContext db,
            [FromRoute] string package,
            CancellationToken cancellationToken = default)
        {
            if (await db.GalleryPackages.CountAsync(x => x.Id == package) == 0)
            {
                return ApiResult<List<GalleryPackage>>(404, "The package has not been found");
            }

            var packages = await db.GalleryPackages
                .Include(x => x.Author)
                .Where(x => x.Id == package)
                .OrderByDescending(x => x.Version)
                .ToListAsync(cancellationToken);

            return ApiResult(packages);
        }

        [HttpGet("{package}/version/{version}")]
        public async ValueTask<ApiResult<GalleryPackage>> GetPackageVersion(
            [FromServices] PipelineContext db,
            [FromRoute] string package,
            [FromRoute] string version,
            CancellationToken cancellationToken = default)
        {
            var _package = await db.GalleryPackages
                .Include(x => x.Author)
                .Where(x => x.Id == package && x.Version == version)
                .SingleOrDefaultAsync(cancellationToken);

            if (_package == null)
            {
                return ApiResult<GalleryPackage>(404, "Package not found");
            }

            return ApiResult(_package);
        }

        [HttpGet("{package}/version/{version}/package.pdo")]
        public async ValueTask<IActionResult> GetPackage(
            [FromServices] PipelineContext db,
            [FromServices] BinaryController binaryController,
            [FromRoute] string package,
            [FromRoute] string version,
            CancellationToken cancellationToken = default)
        {
            var _package = await db.GalleryPackages
                .Where(x => x.Id == package && x.Version == version)
                .SingleOrDefaultAsync(cancellationToken);

            if (_package == null) 
            {
                return NotFound();
            }

            await db.GalleryPackages
                .Where(x => x.Id == package && x.Version == version)
                .SetField(x => x.Downloads).Plus(1)
                .UpdateAsync(cancellationToken);

            // Get the package binary
            return await binaryController.Get(db, _package.PackageBinaryId, cancellationToken);
        }

        [ResponseCache(Duration = 3600 * 24 * 30)]
        [HttpGet("{package}/version/{version}/package.png")]
        public async ValueTask<IActionResult> GetIcon(
            [FromServices] PipelineContext db,
            [FromServices] BinaryController binaryController,
            [FromServices] IWebHostEnvironment env,
            [FromRoute] string package,
            [FromRoute] string version,
            CancellationToken cancellationToken = default)
        {
            var _package = await db.GalleryPackages
                .Where(x => x.Id == package && x.Version == version)
                .SingleOrDefaultAsync(cancellationToken);

            if (_package == null)
            {
                return NotFound();
            }

            if (_package.IconId.HasValue)
            {
                return await binaryController.Get(
                    db, 
                    _package.IconId.Value, 
                    cancellationToken);
            }

            // Return the step icon
            return File(System.IO.File.ReadAllBytes(
                    Path.Combine(env.WebRootPath, "assets", "img", "default.png")), 
                "application/x-png");
        }

        [HttpGet("{package}/version/{version}/{*fileName}")]
        public async ValueTask<IActionResult> GetView(
            [FromServices] PipelineContext db,
            [FromServices] IConfiguration configuration,
            [FromRoute] string package,
            [FromRoute] string version,
            [FromRoute] string fileName,
            CancellationToken cancellationToken = default)
        {
            var _package = await db.GalleryPackages
                .Where(x => x.Id == package && x.Version == version)
                .SingleOrDefaultAsync(cancellationToken);

            if (_package == null)
            {
                return NotFound();
            }

            // Return the step UI from cache
            var cacheFolder = Path.IsPathFullyQualified(configuration["Gallery:CachePath"])
                ? configuration["Gallery:CachePath"]
                : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), configuration["Gallery:CachePath"]);

            cacheFolder = Path.Combine(cacheFolder, package, version);
            var cachePackagePath = cacheFolder + ".pdo";
            if (!Directory.Exists(Path.GetDirectoryName(cachePackagePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cachePackagePath));
            }
            if (!Directory.Exists(Path.GetDirectoryName(cacheFolder)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cacheFolder));
            }
            if (!System.IO.File.Exists(cachePackagePath))
            {
                var bin = await db.Binaries
                    .Include(x => x.Partitions)
                    .SingleOrDefaultAsync(x => x.Id == _package.PackageBinaryId, cancellationToken);

                var bytes = bin.Partitions
                    .OrderBy(x => x.Partition)
                    .SelectMany(x => x.Bytes)
                    .ToArray();

                await System.IO.File.WriteAllBytesAsync(cachePackagePath, bytes, cancellationToken);
            }

            var filePath = Path.Combine(cacheFolder, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                using var fs = new FileStream(cachePackagePath, FileMode.Open, FileAccess.Read);
                using var pdo = new PomeloDevOpsPackage(fs);
                var file = pdo.GetEntry(fileName);
                if (file == null)
                {
                    return NotFound();
                }
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                file.ExtractToFile(filePath, true);
            }

            if (!mimeProvider.TryGetContentType(Path.GetFileName(filePath), out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(new FileStream(filePath, FileMode.Open, FileAccess.Read), contentType, fileName);
        }

        /// <summary>
        /// Upload a gallery package
        /// </summary>
        /// <param name="db"></param>
        /// <param name="package">*.appkg</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async ValueTask<ApiResult<GalleryPackage>> Post(
            [FromServices] PipelineContext db,
            IFormFile package,
            CancellationToken cancellationToken = default)
        {
            using (var stream = package.OpenReadStream())
            using (var appkg = new PomeloDevOpsPackage(stream)) // Read the gallery package which uploaded by user
            {
                var packageBytes= new byte[package.Length];
                var step = await appkg.DeserializeYamlAsync();
                var existed = await db.GalleryPackages
                    .Where(x => x.Id == step.Id)
                    .ToListAsync(cancellationToken);

                // Determine if the current user has the write access
                if (!User.IsInRole(nameof(UserRole.SystemAdmin)) && existed.Any(x => x.AuthorId != Guid.Parse(User.Identity.Name)))
                {
                    return ApiResult<GalleryPackage>(401, $"You don't have the permission to package '{step.Id}'");
                }

                // Check the version if it is already exist.
                if (existed.Any(x => x.Version == step.Version))
                {
                    return ApiResult<GalleryPackage>(400, $"The version {step.Version} of {step.Name} is already existed");
                }

                Binary icon = null;
                try
                {
                    // Storing step icon
                    var iconBytes = await appkg.GetComponentIconAsync();
                    icon = GenerateBinary(iconBytes, $"{step.Id}-{step.Version}.png", "application/x-png");
                }
                catch (FileNotFoundException)
                { 
                    // If the error occurred, keep the icon null for using default icon for the package
                }

                stream.Position = 0;
                var binary = await GenerateBinaryAsync(stream, $"{step.Id}-{step.Version}.appkg", "application/zip", (int)package.Length);
                await UploadBinaryAsync(binary, db, cancellationToken);

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
                // Generate gallery package DB record
                var _package = new GalleryPackage
                {
                    Id = step.Id,
                    Version = step.Version,
                    AuthorId = Guid.Parse(User.Identity.Name),
                    Icon = icon,
                    ListInGallery = true,
                    Description = step.Description,
                    Name = step.Name,
                    Package = binary,
                    Methods = step.Methods == null ? null : string.Join(',', step.Methods.Select(x => x.Name)),
                    Platform = platform
                };
                db.GalleryPackages.Add(_package);
                await db.SaveChangesAsync();

                return ApiResult(_package);
            }
        }

        [Authorize]
        [HttpPut("{package}/version/{version}")]
        [HttpPost("{package}/version/{version}")]
        [HttpPatch("{package}/version/{version}")]
        public async ValueTask<ApiResult<GalleryPackage>> Put(
            [FromServices] PipelineContext db,
            [FromRoute] string package,
            [FromRoute] string version,
            [FromBody] GalleryPackage body,
            CancellationToken cancellationToken = default)
        {
            var _package = await db.GalleryPackages
                .Where(x => x.Id == package 
                    && x.Version == version)
                .SingleOrDefaultAsync(cancellationToken);

            if (_package == null)
            {
                return ApiResult<GalleryPackage>(404, $"The package '{package}-{version}' has not been found");
            }

            if (!User.IsInRole(nameof(UserRole.SystemAdmin)) && _package.AuthorId != Guid.Parse(User.Identity.Name))
            {
                return ApiResult<GalleryPackage>(401, "You don't have the permission to update this package");
            }

            // Patch the author and visibility info of the specified step and version.
            _package.AuthorId = body.AuthorId;
            _package.ListInGallery = body.ListInGallery;

            await db.SaveChangesAsync();
            return ApiResult(_package);
        }

        private async ValueTask<T> GetFromBackupFeedAsync<T>(string endpoint, CancellationToken cancellationToken = default)
            where T : class
        {
            if (_config == null)
            {
                return null;
            }

            var feeds = _config.GetSection("Gallery:AdditionalFeeds").GetChildren().Select(x => x.Value);
            foreach (var x in feeds)
            {
                try 
                {
                    return await GetFromBackupFeedAsync<T>(x, endpoint, cancellationToken);
                }
                catch(KeyNotFoundException)
                {
                    continue;
                }
            }

            return null;
        }

        private async ValueTask<T> GetFromBackupFeedAsync<T>(string feed, string endpoint, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync(feed + endpoint, cancellationToken))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var text = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(text);
                }
                throw new KeyNotFoundException();
            }
        }

        private async ValueTask<byte[]> GetBinaryFromBackupFeedAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            if (_config == null)
            { 
                return null; 
            }

            var feeds = _config.GetSection("Gallery").GetSection("AdditionalFeeds").GetChildren().Select(x => x.Value);
            foreach (var x in feeds)
            {
                try
                {
                    return await GetBinaryFromBackupFeedAsync(x, endpoint, cancellationToken);
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }
            }

            return null;
        }

        private async ValueTask<byte[]> GetBinaryFromBackupFeedAsync(string feed, string endpoint, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync(feed + endpoint, cancellationToken))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                throw new KeyNotFoundException();
            }
        }
    }
}
