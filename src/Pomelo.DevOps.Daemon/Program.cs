// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Pomelo.DevOps.Daemon
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"  _____                     _       
 |  __ \                   | |      
 | |__) |__  _ __ ___   ___| | ___  
 |  ___/ _ \| '_ ` _ \ / _ \ |/ _ \ 
 | |  | (_) | | | | | |  __/ | (_) |
 |_|   \___/|_| |_| |_|\___|_|\___/ 
                                    
                                    ");
            var daemon = new ProcessDaemon();
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => 
            {
                daemon.OnStop();
            };
            daemon.OnStart(args);
            while(true)
            {
                Console.Read();
            }
        }
    }
}
