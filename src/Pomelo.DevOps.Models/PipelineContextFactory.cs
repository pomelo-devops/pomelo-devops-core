using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pomelo.DevOps.Models
{
    public class PipelineContextFactory : IDesignTimeDbContextFactory<PipelineContext>
    {
        public PipelineContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PipelineContext>();
            var connStr = args.Length == 1 ? args[0] : "server=localhost;uid=root;pwd=123456;database=devops";
            builder.UseMySql(connStr, ServerVersion.AutoDetect(connStr), options => options.UseNewtonsoftJson());
            return new PipelineContext(builder.Options);
        }
    }
}
