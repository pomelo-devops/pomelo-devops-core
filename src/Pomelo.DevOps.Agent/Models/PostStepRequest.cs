// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Agent.Models
{
    public class PostStepRequest
    {
        public string Step { get; set; }

        public string Version { get; set; }

        public string Method { get; set; }

        public IDictionary<string, string> Arguments { get; set; }
    }
}
