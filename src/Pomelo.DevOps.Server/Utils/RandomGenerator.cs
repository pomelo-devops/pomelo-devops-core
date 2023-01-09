// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.DevOps.Server.Utils
{
    public static class RandomGenerator
    {
        private static Random random = new Random();

        private const string DefaultDictionary = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";

        public static string Generate(int length, string dictionary = DefaultDictionary)
        {
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; ++i) 
            {
                sb.Append(DefaultDictionary[random.Next(dictionary.Length)]);
            }
            return sb.ToString();
        }
    }
}
