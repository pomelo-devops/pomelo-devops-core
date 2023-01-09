// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.DevOps.Agent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariableController : ControllerBase
    {
        private Connector _connector;
        private VariableContainerFactory _variable;

        public VariableController(
            Connector connector, 
            VariableContainerFactory variable)
        {
            _connector = connector;
            _variable = variable;
        }

        [HttpGet("{container}")]
        public ApiResult<ICollection<string>> Get(Guid container)
        {
            if (!_variable.IsExists(container)) 
            {
                return ApiResult<ICollection<string>>(404, "The variable container is not found");
            }

            return ApiResult(_variable.GetOrCreate(container, null, default, default).Keys);
        }

        [HttpGet("{container}/{name}")]
        public string Get(Guid container, string name)
        {
            if (!_variable.IsExists(container))
            {
                Response.StatusCode = 404;
                return null;
            }
            var _container = _variable.GetOrCreate(container, null, default, default);

            return _container.GetVariable(name);
        }

        [HttpPut("{container}/{name}")]
        [HttpPost("{container}/{name}")]
        [HttpPatch("{container}/{name}")]
        public async ValueTask<ApiResult> Put(
            [FromServices] StageContainer stageContainer,
            [FromRoute] Guid container,
            [FromRoute] string name,
            [FromBody] JobVariable request,
            [FromQuery] bool isLocal = false,
            CancellationToken cancellationToken = default)
        {
            if (!_variable.IsExists(container))
            {
                return ApiResult(404, "The variable container is not found");
            }

            var _container = _variable.GetOrCreate(container, null, default, default);

            try
            {
                _container.PutVariable(name, request.Value);
            }
            catch { }

            if (!isLocal)
            {
                await _connector.SetJobVariableAsync(stageContainer.Get(container), name, request.Value, cancellationToken);
            }
            return ApiResult(200, "Variable has been saved successfully");
        }
    }
}
