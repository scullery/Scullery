namespace Scullery.EntityFrameworkCore
{
    public class SculleryContextFactory : IDesignTimeDbContextFactory<SculleryContext>
    {
        public SculleryContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<SculleryContext>();

            // Tell 'dotnet ef' to consider the test project as the migrations
            // assembly, rather than the default (which is the one with the 
            // DbContext) so that they match.
            builder.UseSqlite("Data Source=scullery.db3", b => b.MigrationsAssembly("Scullery.EntityFrameworkCore.Test"));

            return new SculleryContext(builder.Options);
        }
    }
}