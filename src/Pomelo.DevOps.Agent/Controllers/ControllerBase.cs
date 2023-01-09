// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pomelo.DevOps.Models.ViewModels;
using YamlDotNet.Serialization;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Pomelo.DevOps.Agent.Controllers
{
    public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        protected static SHA256 SHA256 = SHA256.Create();
        private ISerializer yamlSerializer;
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
            if (Response != null)
            {
                Response.StatusCode = code;
            }
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
    }
}
