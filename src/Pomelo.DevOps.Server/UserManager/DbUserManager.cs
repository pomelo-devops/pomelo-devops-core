// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Pomelo.DevOps.Models;
using Pomelo.DevOps.Server.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.DevOps.Server.UserManager
{
    public class DbUserManager
    {
        private PipelineContext _db;

        public DbUserManager(PipelineContext db)
        {
            _db = db;
        }

        public async ValueTask<bool> ValidateUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            var _user = await _db.Users
                   .Where(x => x.Username == username 
                    && x.LoginProvider.Mode == LoginProviderType.Local
                    && x.LoginProvider.Enabled)
                   .SingleOrDefaultAsync(cancellationToken);

            if (_user == null)
            {
                return false;
            }

            var hash = Crypto.ComputeSha256Hash(Encoding.ASCII.GetBytes(password), _user.Salt);
            if (!hash.SequenceEqual(_user.PasswordHash))
            {
                return false;
            }

            return true;
        }
    }

    public static class DbUserManagerExtensions
    {
        public static IServiceCollection AddDbUserManager(this IServiceCollection collection)
        {
            return collection.AddScoped<DbUserManager>();
        }
    }
}
