// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pomelo.DevOps.JobExtensions.TRX.Models;
using Pomelo.DevOps.Models.ViewModels;
using System.Xml.Linq;

namespace Pomelo.DevOps.JobExtensions.TRX.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TrxController : ControllerBase
    {
        private const string InvalidFileNameChars = "/\\:,*?\"<>|";

        [HttpGet]
        public ApiResult<IEnumerable<string>> GetSuites()
        {
            if (!IsTrxFolderExists)
            {
                return ApiResult<IEnumerable<string>>(404, "No test suite found");
            }

            return ApiResult(Directory.EnumerateFiles(TrxFolderPath, "*.trx", SearchOption.TopDirectoryOnly)
                .Select(x => Path.GetFileNameWithoutExtension(x)));
        }

        [HttpGet("{suite}")]
        public async ValueTask<ApiResult<List<TrxCase>>> Get(
            [FromRoute] string suite,
            CancellationToken cancellationToken)
        {
            if (!IsTrxFolderExists)
            {
                return ApiResult<List<TrxCase>>(404, "No test case found");
            }

            var trxPath = Path.Combine(TrxFolderPath, suite + ".trx");
            if (!System.IO.File.Exists(trxPath))
            {
                return ApiResult<List<TrxCase>>(404, "No test case found");
            }

            var ret = ParseTrxContent(await System.IO.File.ReadAllTextAsync(trxPath, cancellationToken));
            return ApiResult(ret);
        }

        [HttpPut("{suite}")]
        [HttpPost("{suite}")]
        public async ValueTask<ApiResult<List<TrxCase>>> Post(
            [FromRoute] string suite,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            using var stream = file.OpenReadStream();
            using var streamReader = new StreamReader(stream);
            var trxContent = await streamReader.ReadToEndAsync();

            List<TrxCase> ret = null;
            try
            {
                ret = ParseTrxContent(trxContent);
            }
            catch (JsonException)
            { }

            if (ret == null)
            {
                return ApiResult<List<TrxCase>>(400, "Invalid trx content");
            }

            var fileName = suite + ".trx";
            if (fileName.Any(x => InvalidFileNameChars.Contains(x)))
            {
                return ApiResult<List<TrxCase>>(400, "Invalid suite name");
            }

            var trxPath = Path.Combine(TrxFolderPath, fileName);
            if (!Directory.Exists(TrxFolderPath))
            {
                Directory.CreateDirectory(TrxFolderPath);
            }
            await System.IO.File.WriteAllTextAsync(trxPath, trxContent);
            return ApiResult(ret);
        }

        private List<TrxCase> ParseTrxContent(string trxContent)
        {
            var document = XDocument.Parse(trxContent);
            var nsPrefix = "";
            var xmlnsAttribute = document.Root.Attribute("xmlns");
            if (xmlnsAttribute != null)
            {
                nsPrefix = $"{{{xmlnsAttribute.Value}}}";
            }
            var definitions = document.Descendants(nsPrefix + "UnitTest").ToList();
            var ret = new List<TrxCase>();
            foreach (var def in definitions)
            {
                var execution = def.Elements(nsPrefix + "Execution").First();
                var method = def.Elements(nsPrefix + "TestMethod").First();
                var executionId = Guid.Parse(execution.Attribute("id").Value);
                var result = document.Descendants(nsPrefix + "UnitTestResult")
                    .FirstOrDefault(x => Guid.Parse(x.Attribute("executionId").Value) == executionId);
                TrxTestResult testResult = null;
                if (result != null)
                {
                    testResult = new TrxTestResult
                    {
                        ComputerName = result.Attribute("computerName").Value,
                        Duration = TimeSpan.Parse(result.Attribute("duration").Value),
                        StartTime = DateTime.Parse(result.Attribute("startTime").Value),
                        EndTime = DateTime.Parse(result.Attribute("endTime").Value),
                        Outcome = result.Attribute("outcome").Value
                    };

                    var output = result.Element(nsPrefix + "Output");
                    if (output != null)
                    {
                        var errorInfo = output.Element(nsPrefix + "ErrorInfo");
                        if (errorInfo != null)
                        {
                            var message = errorInfo.Element(nsPrefix + "Message");
                            if (message != null)
                            {
                                testResult.Message = message.Value;
                            }

                            var stackTrace = errorInfo.Element(nsPrefix + "StackTrace");
                            if (stackTrace != null)
                            {
                                testResult.StackTrace = stackTrace.Value;
                            }
                        }

                        var stdOut = output.Element(nsPrefix + "StdOut");
                        if (stdOut != null)
                        {
                            testResult.StdOut = stdOut.Value;
                        }
                    }
                }
                var test = new TrxCase
                {
                    Id = Guid.Parse(def.Attribute("id").Value),
                    Storage = def.Attribute("storage").Value,
                    CodeBase = method.Attribute("codeBase").Value,
                    AdapterName = method.Attribute("adapterTypeName").Value,
                    ClassName = method.Attribute("className").Value,
                    MethodName = method.Attribute("name").Value,
                    FullName = def.Attribute("name").Value,
                    TestResult = testResult
                };

                ret.Add(test);
            }

            return ret;
        }

        private string TrxFolderPath
            => Path.Combine(HttpContext.RequestServices.GetRequiredService<IConfiguration>()["CaseStorage"], $"{PipelineId}-{JobNumber}");

        private bool IsTrxFolderExists
            => System.IO.Directory.Exists(TrxFolderPath);
    }
}
