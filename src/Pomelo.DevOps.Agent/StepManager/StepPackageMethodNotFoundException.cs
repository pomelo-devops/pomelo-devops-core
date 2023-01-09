// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Pomelo.DevOps.Agent
{
    public class StepPackageMethodNotFoundException : Exception
    {
        public string Package { get; private set; }
        public string Version { get; private set; }
        public string Method { get; private set; }

        public StepPackageMethodNotFoundException(string package, string version, string method)
            : base($"The speicified method has not been found")
        {
            Package = package;
            Version = version;
            Method = method;
        }
    }
}
