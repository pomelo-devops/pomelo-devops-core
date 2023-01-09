// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Pomelo.DevOps.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Pomelo.DevOps.Server.Authentication
{
    [ExcludeFromCodeCoverage]
    public class TokenAuthenticateHandler : AuthenticationHandler<TokenOptions>
    {
        public new const string Scheme = "Token";

        private PipelineContext _db;

        public TokenAuthenticateHandler(
            IOptionsMonitor<TokenOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            PipelineContext db)
            : base(options, logger, encoder, clock)
        {
            this._db = db;
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
                else if (!string.IsNullOrEmpty(Request.Query["session"]))
                {
                    authorization = new[] { $"Session {Request.Query["session"]}" };
                }
                else
                {
                    return AuthenticateResult.NoResult();
                }
            }

            User tokenOwner = null;
            if (authorization.First().StartsWith("Token", StringComparison.OrdinalIgnoreCase))
            {
                var t = authorization.First().Substring("Token ".Length);
                var pat = await _db.PersonalAccessTokens
                 .Include(x => x.User)
                 .Where(x => x.Token == t && (!x.ExpireAt.HasValue || x.ExpireAt.Value > DateTime.UtcNow))
                 .SingleOrDefaultAsync();

                if (pat == null)
                {
                    return AuthenticateResult.NoResult();
                }

                tokenOwner = pat.User;
            }
            else if (authorization.First().StartsWith("Session", StringComparison.OrdinalIgnoreCase))
            { 
                var t = authorization.First().Substring("Session ".Length);
                var session = await _db.UserSessions
                    .Include(x => x.User)
                    .Where(x => x.Id == t)
                    .SingleOrDefaultAsync();
                if (session == null)
                {
                    return AuthenticateResult.NoResult();
                }
                if (session.ExpireAt <= DateTime.UtcNow)
                {
                    return AuthenticateResult.Fail(new TokenExpiredException(session.Id));
                }
                tokenOwner = session.User;
                session.ExpireAt = DateTime.UtcNow.AddMinutes(20);
            }
            else
            {
                return AuthenticateResult.NoResult();
            }

            var claimIdentity = new ClaimsIdentity(Scheme, ClaimTypes.Name, ClaimTypes.Role);
            claimIdentity.AddClaim(new Claim(ClaimTypes.Name, tokenOwner.Id.ToString()));
            claimIdentity.AddClaim(new Claim(ClaimTypes.Role, tokenOwner.Role.ToString()));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimIdentity), Scheme);
            await _db.SaveChangesAsync();

            return AuthenticateResult.Success(ticket);
        }
    }
}
