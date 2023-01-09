﻿// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Pomelo.DevOps.JobExtensions.TRX.Authentication
{
    [ExcludeFromCodeCoverage]
    public class TokenPostConfigureOptions : IPostConfigureOptions<TokenOptions>
    {
        public void PostConfigure(string name, TokenOptions options)
        {
        }
    }
}