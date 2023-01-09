// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Pomelo.DevOps.WindowsService
{
    public class Daemon
    {
        public string FileName { get; set; }

        public string WorkingDirectory { get; set; }

        public string Arguments { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string LogsOutputDirectory { get; set; }
    }
}
