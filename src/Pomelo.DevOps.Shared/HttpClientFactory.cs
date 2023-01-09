// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Net.Http;

namespace Pomelo.DevOps.Shared
{
    public static class HttpClientFactory
    {
        public static readonly HttpClientHandler IgnoreSslErrorHandler = new HttpClientHandler 
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        public static HttpClient CreateHttpClient(bool ignoreSslErrors = false)
        { 
            if (!ignoreSslErrors)
            {
                return new HttpClient() { Timeout = new System.TimeSpan(0, 0, 10) };
            }
            else
            {
                return new HttpClient(IgnoreSslErrorHandler) { Timeout = new System.TimeSpan(0, 0, 10) };
            }
        }
    }
}
