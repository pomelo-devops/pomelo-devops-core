// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Pomelo.DevOps.Server.Authentication
{
    [ExcludeFromCodeCoverage]
    public static class TokenAuthenticateExtensions
    {
        public static AuthenticationBuilder AddPersonalAccessToken(
            this AuthenticationBuilder builder)
            => builder.AddPomeloToken(TokenAuthenticateHandler.Scheme, null, _ => { });

        public static AuthenticationBuilder AddPomeloToken(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            string displayName,
            Action<TokenOptions> configureOptions)
        {
            builder
                .Services
                .TryAddEnumerable(
                    ServiceDescriptor.Singleton<IPostConfigureOptions<TokenOptions>, TokenPostConfigureOptions>());

            return builder.AddScheme<TokenOptions, TokenAuthenticateHandler>(
                authenticationScheme,
                displayName,
                configureOptions);
        }
    }
}