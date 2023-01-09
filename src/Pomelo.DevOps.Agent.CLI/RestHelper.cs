// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Text;
using Newtonsoft.Json;

namespace Pomelo.DevOps.Agent.CLI
{
    internal static class RestHelper
    {
        static HttpClient client = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };
        public static async ValueTask<bool> PostAsync(string url, object body)
        {
            using var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine(await response.Content.ReadAsStringAsync());
                Environment.Exit(1);
                return false;
            }

            return true;
        }
    }
}
