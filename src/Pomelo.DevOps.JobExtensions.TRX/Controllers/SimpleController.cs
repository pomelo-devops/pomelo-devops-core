// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pomelo.DevOps.JobExtensions.TRX.Models;
using Pomelo.DevOps.Models.ViewModels;

namespace Pomelo.DevOps.JobExtensions.TRX.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SimpleController : ControllerBase
    {
        private const string InvalidFileNameChars = "/\\:,*?\"<>|";

        [HttpGet]
        public ApiResult<IEnumerable<string>> GetSuites()
        {
            if (!IsSimpleFolderExists)
            {
                return ApiResult<IEnumerable<string>>(404, "No test suite found");
            }

            return ApiResult(Directory.EnumerateFiles(SimpleFolderPath, "*.json", SearchOption.TopDirectoryOnly)
                .Select(x => Path.GetFileNameWithoutExtension(x)));
        }

        [HttpGet("{suite}")]
        public async ValueTask<ApiResult<List<SimpleCase>>> Get(
            [FromRoute] string suite,
            CancellationToken cancellationToken)
        {
            if (!IsSimpleFolderExists)
            {
                return ApiResult<List<SimpleCase>>(404, "No test case found");
            }

            var SimplePath = Path.Combine(SimpleFolderPath, suite + ".json");
            if (!System.IO.File.Exists(SimplePath))
            {
                return ApiResult<List<SimpleCase>>(404, "No test case found");
            }

            var ret = JsonConvert.DeserializeObject<List<SimpleCase>>((await System.IO.File.ReadAllTextAsync(SimplePath, cancellationToken)));
            return ApiResult(ret);
        }

        [HttpPut("{suite}")]
        [HttpPost("{suite}")]
        public async ValueTask<ApiResult<List<SimpleCase>>> Post(
            [FromRoute] string suite,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            using var stream = file.OpenReadStream();
            using var streamReader = new StreamReader(stream);
            var SimpleContent = await streamReader.ReadToEndAsync();

            List<SimpleCase> ret = null;
            try
            {
                ret = JsonConvert.DeserializeObject<List<SimpleCase>>(SimpleContent);
            }
            catch (JsonException)
            { }

            if (ret == null)
            {
                return ApiResult<List<SimpleCase>>(400, "Invalid Simple content");
            }

            var fileName = suite + ".json";
            if (fileName.Any(x => InvalidFileNameChars.Contains(x)))
            {
                return ApiResult<List<SimpleCase>>(400, "Invalid suite name");
            }

            var SimplePath = Path.Combine(SimpleFolderPath, fileName);
            if (!Directory.Exists(SimpleFolderPath))
            {
                Directory.CreateDirectory(SimpleFolderPath);
            }
            await System.IO.File.WriteAllTextAsync(SimplePath, SimpleContent);
            return ApiResult(ret);
        }

        private string SimpleFolderPath
            => Path.Combine(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["CaseStorage"], $"{PipelineId}-{JobNumber}");

        private bool IsSimpleFolderExists
            => System.IO.Directory.Exists(SimpleFolderPath);
    }
}
