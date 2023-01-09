// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.DevOps.Models.ViewModels;
using Pomelo.DevOps.Models;
using Pomelo.DevOps.Server.Utils;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Pomelo.DevOps.Models.LoginProviders;

namespace Pomelo.DevOps.Server.Controllers
{
    [Route("api/[controller]/{providerId}")]
    [ApiController]
    public class OAuth2Controller : ControllerBase
    {
        private static HttpClient client = new HttpClient();

        [HttpPost("code")]
        public async ValueTask<ApiResult<UserSession>> PostCode(
            [FromServices] PipelineContext db,
            [FromServices] ITokenGenerator tokenGenerator,
            [FromBody] PostOAuth2CodeRequest request,
            [FromRoute] string providerId,
            [FromQuery] string redirect,
            CancellationToken cancellationToken = default)
        {
            var _provider = await db.LoginProviders
                .FirstOrDefaultAsync(x => x.Id == providerId, cancellationToken);

            if (_provider == null)
            {
                return ApiResult<UserSession>(404, "Login provider not found");
            }

            var provider = LoginProviderController.CombineProvider<OAuth2Login>(_provider);

            if (!provider.Enabled)
            {
                return ApiResult<UserSession>(404, "Login provider not enabled");
            }

            // 1. Get Access Token
            var form = new Dictionary<string, string>
            {
                ["code"] = request.Code.ToString(),
                ["client_id"] = provider.Redirection.ClientId,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirect
            };

            if (!string.IsNullOrWhiteSpace(provider.ClientSecret))
            {
                form["client_secret"] = provider.ClientSecret;
            }

            using var content1 = new FormUrlEncodedContent(form);
            using var response = await client.PostAsync(CombineUrl(provider.ServerBaseUrl, provider.AccessToken.Endpoint), content1);
            var jsonText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<UserSession>(400, jsonText);
            }

            var responseObject = JsonConvert.DeserializeObject<JObject>(jsonText);
            var _accessToken = responseObject.SelectToken(provider.AccessToken.AccessTokenPath, false);
            var accessToken = _accessToken?.Value<string>();

            var tokenType = "Bearer";
            if (provider.AccessToken.TokenTypePath != null)
            {
                var _tokenType = responseObject.SelectToken(provider.AccessToken.TokenTypePath);
                tokenType = _tokenType?.Value<string>();
            }

            // 2. Get User Info
            using var message = new HttpRequestMessage(HttpMethod.Get, CombineUrl(provider.ServerBaseUrl, provider.UserInfo.Endpoint));
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(tokenType, accessToken);
            using var response2 = await client.SendAsync(message, cancellationToken);
            var jsonText2 = await response2.Content.ReadAsStringAsync();
            if (!response2.IsSuccessStatusCode)
            {
                return ApiResult<UserSession>(400, jsonText);
            }
            var responseObject2 = JsonConvert.DeserializeObject<JObject>(jsonText2);

            var _username = responseObject2.SelectToken(provider.UserInfo.UsernamePath);
            var username = _username?.Value<string>();

            var _email = responseObject2.SelectToken(provider.UserInfo.EmailPath);
            var email = _email?.Value<string>();

            var _displayName = responseObject2.SelectToken(provider.UserInfo.DisplayNamePath);
            var displayName = _displayName?.Value<string>();

            // Get or create user
            var user = await db.Users.FirstOrDefaultAsync(x => x.Username == username && x.LoginProviderId == provider.Id);
            if (user == null)
            {
                user = new User
                {
                    Username = username,
                    Email = email,
                    Salt = new byte[0],
                    PasswordHash = new byte[0],
                    Role = UserRole.Collaborator,
                    DisplayName = displayName,
                    LoginProviderId = provider.Id
                };
                db.Users.Add(user);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Update user
                user.Email = email;
                displayName = user.DisplayName;
            }

            while (true)
            {
                // If the token conflicted, we need to retry
                // TODO: Collect expired tokens
                UserSession session = null;
                try
                {
                    var bytes = tokenGenerator.Generate(32);
                    session = new UserSession
                    {
                        Id = Convert.ToBase64String(bytes),
                        Address = HttpContext.Connection.RemoteIpAddress.ToString(),
                        Agent = HttpContext.Request.Headers["User-Agent"].ToString(),
                        UserId = user.Id
                    };
                    db.UserSessions.Add(session);
                    await db.SaveChangesAsync(cancellationToken);
                    session.User = user;
                    return ApiResult(session);
                }
                catch (Exception ex)
                {
                    if (session != null)
                    {
                        db.UserSessions.Remove(session);
                    }

                    var exceptionTypes = new Type[] { typeof(DbUpdateException), typeof(InvalidOperationException), typeof(ArgumentException) };
                    if (!exceptionTypes.Contains(ex.GetType()))
                    {
                        throw;
                    }
                }
            }
        }

        private static string CombineUrl(string baseUrl, string endpoint)
        { 
            if (endpoint.Contains("http://") || endpoint.Contains("https://")) 
            {
                return endpoint;
            }
            else
            {
                return baseUrl + endpoint;
            }
        }

        /* TODO: Integration with PKCE
        [HttpGet("pkce")]
        public async ValueTask<ApiResult<string>> GetPkce(
            [FromQuery] string verifier)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(verifier));
            var b64Hash = Convert.ToBase64String(hash);
            var code = Regex.Replace(b64Hash, "\\+", "-");
            code = Regex.Replace(code, "\\/", "_");
            code = Regex.Replace(code, "=+$", "");
            return ApiResult<string>(code);
        }
        */
    }
}
