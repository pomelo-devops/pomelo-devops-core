// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Pomelo.DevOps.JobExtensions.TRX.Authentication
{
    [ExcludeFromCodeCoverage]
    public class TokenAuthenticateHandler : AuthenticationHandler<TokenOptions>
    {
        public new const string Scheme = "Token";
        private IConfiguration config;

        public TokenAuthenticateHandler(
            IOptionsMonitor<TokenOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config)
            : base(options, logger, encoder, clock)
        {
            this.config = config;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorization = Request.Headers["Authorization"].ToArray();
            if (authorization.Length == 0)
            {
                if (!string.IsNullOrEmpty(Request.Query["token"]))
                {
                    authorization = new[] { $"Token {Request.Query["token"]}" };
                }
                else
                {
                    return AuthenticateResult.NoResult();
                }
            }

            if (authorization.First().StartsWith("Token", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorization.First().Substring("Token ".Length);

                if (token != config["ProviderToken"])
                {
                    return AuthenticateResult.Fail("Invalid credential");
                }
            }
            else
            {
                return AuthenticateResult.NoResult();
            }

            var claimIdentity = new ClaimsIdentity(Scheme, ClaimTypes.Name, ClaimTypes.Role);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimIdentity), Scheme);

            return AuthenticateResult.Success(ticket);
        }
    }
}
