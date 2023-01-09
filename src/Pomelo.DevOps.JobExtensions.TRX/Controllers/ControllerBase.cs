// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pomelo.DevOps.Models.ViewModels;

namespace Pomelo.DevOps.JobExtensions.TRX.Controllers
{
    [Authorize]
    public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        protected string PipelineId => Request.Headers.Keys.Contains("X-Pomelo-Pipeline") 
            ? Request.Headers["X-Pomelo-Pipeline"].ToString()
            : null;

        protected long? JobNumber => Request.Headers.Keys.Contains("X-Pomelo-Job-Number")
            ? Convert.ToInt64(Request.Headers["X-Pomelo-Job-Number"].ToString())
            : null;

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
    }
}
