// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq;
using System.Security.Cryptography;

namespace Pomelo.DevOps.Server.Utils
{
    public static class Crypto
    {
        static SHA256 SHA256 = SHA256.Create();

        public static byte[] ComputeSha256Hash(byte[] value, byte[] salt)
        {
            return SHA256.ComputeHash(value.Concat(salt).ToArray(), 0, value.Length + salt.Length);
        }
    }
}
