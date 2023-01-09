// Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Pomelo.DevOps.Triggers.GitLab.Models
{
    public class TriggerContext : DbContext
    {
        public TriggerContext(DbContextOptions<TriggerContext> opt) : base(opt)
        { }

        public DbSet<Trigger> Triggers { get; set; }

        public DbSet<TriggerHistory> TriggerHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Trigger>(e =>
            {
                e.HasIndex(x => new { x.Enabled, x.Type, x.GitLabNamespace, x.GitLabProject });
            });

            builder.Entity<TriggerHistory>(e => 
            {
                e.HasKey(x => new { x.Type, x.NamespaceProject, x.CommitHash });
            });
        }
    }
}
