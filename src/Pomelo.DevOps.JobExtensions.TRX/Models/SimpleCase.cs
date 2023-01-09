// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Pomelo.DevOps.JobExtensions.TRX.Models
{
    public enum CaseResult
    {
        Passed, 
        Failure,
        Skipped,
        Timeout
    }

    public class SimpleCase
    {
        public string Name { get; set; }

        public CaseResult Result { get; set; }

        public TimeSpan Duration { get; set; }

        public string Message { get; set; }
    }
}
