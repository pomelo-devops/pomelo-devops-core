// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Pomelo.DevOps.JobExtensions.TRX.Models
{
    public class TrxTestResult
    { 
        public string ComputerName { get; set; }

        public TimeSpan Duration { get; set; } 

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Outcome { get; set; }

        public string StdOut { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }
    }

    public class TrxCase
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }

        public string CodeBase { get; set; }

        public string Storage { get; set; }

        public string AdapterName { get; set; }

        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public TrxTestResult TestResult { get; set; }
    }
}
