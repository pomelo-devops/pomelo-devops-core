// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pomelo.DevOps.Shared
{
    public static class HttpRequestExtensions
    {
        private static readonly string[] methodsHasBody = new[] { "POST", "PUT", "PATCH" }; 

        public static HttpRequestMessage ToHttpRequestMessage(
            this HttpRequest request,
            string toUrl,
            IDictionary<string, string> headers = null)
        {
            var qs = request.QueryString.ToString();
            if (!string.IsNullOrWhiteSpace(qs))
            {
                toUrl = toUrl + "?" + qs;
            }

            var ret = new HttpRequestMessage();
            ret.Method = new HttpMethod(request.Method);
            ret.RequestUri = new Uri(toUrl);

            if (methodsHasBody.Contains(ret.Method.Method.ToUpper()))
            {
                if (request.ContentType != null && request.ContentType.Contains("multipart", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.ContentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NotSupportedException(request.ContentType);
                    }

                    var content = new MultipartFormDataContent();
                    foreach (var x in request.Form.Files)
                    {
                        content.Add(new StreamContent(x.OpenReadStream()), x.Name, x.FileName);
                    }

                    if (request.Form.Keys.Count > 0)
                    {
                        content.Add(new FormUrlEncodedContent(request.Form.Keys
                            .Select(x => new KeyValuePair<string, string>(x, request.Form[x]))));
                    }

                    ret.Content = content;
                }
                else
                {
                    var content = new StreamContent(request.Body);

                    // Patch Content Headers
                    foreach (var header in request.Headers)
                    {
                        try
                        {
                            if (content.Headers.Contains(header.Key))
                            {
                                content.Headers.Remove(header.Key);
                            }
                            content.Headers.Add(header.Key, header.Value.ToArray());
                        }
                        catch (InvalidOperationException) { }
                        catch (FormatException) { }
                    }

                    ret.Content = content;
                }
            }

            // Patch Request Headers
            foreach (var header in request.Headers)
            {
                try
                {
                    if (ret.Headers.Contains(header.Key))
                    {
                        ret.Headers.Remove(header.Key);
                    }
                    ret.Headers.Add(header.Key, header.Value.ToArray());
                }
                catch (InvalidOperationException) { }
                catch (FormatException) { }
            }

            // Patch Additional Headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    try
                    {
                        if (ret.Headers.Contains(header.Key))
                        {
                            ret.Headers.Remove(header.Key);
                        }
                        ret.Headers.Add(header.Key, header.Value);
                    }
                    catch (InvalidOperationException) { }
                    catch (FormatException) { }
                }
            }

            return ret;
        }

        public static async ValueTask ConsumeHttpResponseMessageAsync(
            this HttpResponse response,
            HttpResponseMessage message,
            CancellationToken cancellationToken = default)
        {
            response.StatusCode = (int)message.StatusCode;
            foreach (var header in message.Headers)
            {
                if (header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    response.Headers[header.Key] = header.Value.ToArray();
                }
                catch (InvalidOperationException) { }
                catch (FormatException) { }
            }

            foreach (var header in message.Content.Headers)
            {
                if (header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    response.Headers[header.Key] = header.Value.ToArray();
                }
                catch (InvalidOperationException) { }
                catch (FormatException) { }
            }
            using var stream = message.Content.ReadAsStream(cancellationToken);
            await stream.CopyToAsync(response.Body, cancellationToken);
        }
    }
}
