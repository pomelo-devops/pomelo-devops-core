// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Pomelo.DevOps.Triggers.Scheduled.Models
{
    public class TriggerContext : DbContext
    {
        public TriggerContext(DbContextOptions<TriggerContext> opt) : base(opt)
        { }

        public DbSet<Trigger> Triggers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Trigger>(e =>
            {
                e.HasIndex(x => x.Enabled);
            });
        }
    }
}
