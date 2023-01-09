// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using YamlDotNet.Serialization;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Pomelo.DevOps.Server.Controllers
{
    public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        protected static SHA256 SHA256 = SHA256.Create();
        private ISerializer yamlSerializer;
        protected static object JobLocker = new object();
        protected Regex CommonIdRegex = new Regex("^[a-zA-Z0-9-_]{1,}$");

        protected virtual ISerializer YamlSerializer
        {
            get
            {
                if (yamlSerializer == null)
                {
                    yamlSerializer = new SerializerBuilder().Build();
                }
                return yamlSerializer;
            }
        }

        private IDeserializer yamlDeserializer;
        protected virtual IDeserializer YamlDeserializer
        {
            get 
            {
                if (yamlDeserializer == null)
                {
                    yamlDeserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                }
                return yamlDeserializer;
            }
        }

        protected ApiResult ApiResult(int code = 200, string message = null)
        {
            message = message ?? "Succeeded";
            Response.StatusCode = code;
            return new ApiResult
            {
                Code = code,
                Message = message
            };
        }

        protected ApiResult<T> ApiResult<T>(T data)
        {
            return new ApiResult<T>
            {
                Code = 200,
                Message = "Succeeded",
                Data = data
            };
        }

        protected ApiResult<T> ApiResult<T>(int code = 200, string message = null)
        {
            message = message ?? "Succeeded";
            Response.StatusCode = code;
            return new ApiResult<T>
            {
                Code = code,
                Message = message
            };
        }

        protected PagedApiResult PagedApiResult(int code = 200, string message = null)
        {
            message = message ?? "Succeeded";
            Response.StatusCode = code;
            return new PagedApiResult
            {
                Code = code,
                Message = message
            };
        }

        protected async ValueTask<PagedApiResult<T>> PagedApiResultAsync<T>(
            IQueryable<T> data,
            int currentPage,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var totalRecords = await data.CountAsync(cancellationToken);
            data = data.Skip((currentPage - 1) * pageSize).Take(pageSize);

            return new PagedApiResult<T>
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

        protected PagedApiResult<T> PagedApiResult<T>(int code = 200, string message = null)
        {
            message = message ?? "Succeeded";
            Response.StatusCode = code;
            return new PagedApiResult<T>
            {
                Code = code,
                Message = message
            };
        }

        /// <summary>
        /// Determine the user has the permission to the specified project
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="role">Project member role</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async ValueTask<bool> HasPermissionToProjectAsync(
            PipelineContext db,
            string projectId,
            ProjectMemberRole role,
            CancellationToken cancellationToken = default)
        {
            if (User == null || User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                return true;
            }

            if (await db.ProjectMembers.AnyAsync(x => x.UserId == Guid.Parse(User.Identity.Name)
                && x.ProjectId == projectId
                && x.Role >= role, cancellationToken))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determine the user has the permission to the specified pipeline
        /// </summary>
        /// <param name="db"></param>
        /// <param name="projectId">Project ID</param>
        /// <param name="role">Project member role</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected virtual async ValueTask<bool> HasPermissionToPipelineAsync(
            PipelineContext db,
            string projectId,
            string pipeline,
            PipelineAccessType access,
            CancellationToken cancellationToken = default)
        {
            if (User == null || User.IsInRole(nameof(UserRole.SystemAdmin)))
            {
                return true;
            }

            if (await db.ProjectMembers.AnyAsync(
                x => x.UserId == Guid.Parse(User.Identity.Name)
                    && x.ProjectId == projectId
                    && x.Role >= ProjectMemberRole.Agent, 
                cancellationToken))
            {
                return true;
            }

            if (access == PipelineAccessType.Reader)
            {
                if (await db.ProjectMembers.AnyAsync(x => x.UserId == Guid.Parse(User.Identity.Name)
                    && x.ProjectId == projectId
                    && x.Role >= ProjectMemberRole.Member,
                    cancellationToken) && await db.Pipelines.AnyAsync(x => x.Id == pipeline
                    && x.Visibility == PipelineVisibility.Public)) 
                {
                    return true;
                }
            }

            if (await db.PipelineAccesses.AnyAsync(
                x => x.UserId == Guid.Parse(User.Identity.Name)
                    && x.PipelineId == pipeline
                    && x.AccessType >= access, cancellationToken))
            {
                return true;
            }

            return false;
        }

        protected async ValueTask<string> GetPolicyValueAsync(
            PipelineContext db, 
            string key, 
            CancellationToken cancellationToken = default)
        {
            return (await db.Policies.FirstOrDefaultAsync(x => x.Key == key, cancellationToken)).ValueJson;
        }

        private static int AgentVersionCacheCounter = 101;
        private static string AgentVersionCache = null;

        protected async ValueTask<bool> DetermineAgentVersionAsync(CancellationToken cancellationToken) 
        {
            if (AgentVersionCacheCounter > 100)
            {
                var buildFile = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "wwwroot",
                "agent",
                "build.txt");

                if (!System.IO.File.Exists(buildFile))
                {
                    AgentVersionCache = null;
                }
                else
                {
                    AgentVersionCache = (await System.IO.File.ReadAllTextAsync(buildFile, cancellationToken)).Trim();
                }

                AgentVersionCacheCounter = 0;
            }
            
            if (!Request.Headers.ContainsKey("X-Pomelo-Agent-Version") 
                && AgentVersionCache != null)
            {
                return false;
            }

            ++AgentVersionCacheCounter;
            return Request.Headers["X-Pomelo-Agent-Version"].FirstOrDefault() == AgentVersionCache;
        }

        public static Binary GenerateBinary(byte[] bytes, string filename, string contentType)
        {
            return new Binary
            {
                ContentType = contentType,
                Filename = filename,
                Size = bytes.Length,
                Sha256 = Convert.ToBase64String(SHA256.ComputeHash(bytes)),
                Partitions = bytes.Select((x, i) => new { Partition = i / (1024 * 1024), Byte = x })
                    .GroupBy(x => x.Partition)
                    .Select(x => new BinaryPartition
                    {
                        Partition = x.Key,
                        Bytes = x.Select(x => x.Byte).ToArray()
                    })
                    .ToList()
            };
        }

        public static async ValueTask UploadBinaryAsync(Binary binary, PipelineContext db, CancellationToken cancellationToken = default)
        {
            var partitions = binary.Partitions;
            binary.Partitions = null;
            db.Binaries.Add(binary);
            await db.SaveChangesAsync(cancellationToken);
            foreach (var x in partitions)
            {
                x.BinaryId = binary.Id;
                db.BinaryPartitions.Add(x);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public static async ValueTask<Binary> GenerateBinaryAsync(Stream stream, string filename, string contentType, int length)
        {
            var bytes = new byte[length];
            var read = 0;
            while(read < length)
            {
                read += await stream.ReadAsync(bytes, read, length - read);
            }
            return GenerateBinary(bytes, filename, contentType);
        }
    }
}
