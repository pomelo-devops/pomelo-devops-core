// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Pomelo.DevOps.JobExtensions.TRX.Authentication
{
    [ExcludeFromCodeCoverage]
    public class TokenExpiredException : Exception
    {
        public string Token { get; private set; }

        public TokenExpiredException(string token) : base($"Token {token} is already expired.")
        {
            this.Token = token;
        }
    }
}
