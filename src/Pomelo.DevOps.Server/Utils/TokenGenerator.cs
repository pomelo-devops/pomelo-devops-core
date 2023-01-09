// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.DevOps.Server.Utils
{
    public interface ITokenGenerator
    {
        byte[] Generate(int length);
    }

    public class TokenGenerator : ITokenGenerator
    {
        private Random random = new Random();

        public byte[] Generate(int length)
        {
            var bytes = new byte[length];
            random.NextBytes(bytes);
            return bytes;
        }
    }

    public static class TokenGeneratorExtensions
    {
        public static IServiceCollection AddTokenGenerator(this IServiceCollection collection)
        {
            return collection.AddSingleton<ITokenGenerator, TokenGenerator>();
        }
    }
}
