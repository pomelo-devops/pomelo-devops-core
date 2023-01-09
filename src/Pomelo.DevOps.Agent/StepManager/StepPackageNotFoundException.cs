// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Pomelo.DevOps.Agent
{
    public class StepPackageNotFoundException : Exception
    {
        public IEnumerable<string> Feeds { get; private set; }
        public string Package { get; private set; }
        public string Version { get; private set; }

        public StepPackageNotFoundException(IEnumerable<string> feeds, string package, string version)
            : base($"The speicified package has not been found")
        {
            Feeds = feeds;
            Package = package;
            Version = version;
        }
    }
}
