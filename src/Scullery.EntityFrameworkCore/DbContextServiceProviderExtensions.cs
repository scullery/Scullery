namespace Microsoft.Extensions.DependencyInjection;

static class DbContextServiceProviderExtensions
{
    public static void InitializeDatabase<T>(this IServiceProvider serviceProvider) where T : DbContext
    {
        using (var serviceScope = serviceProvider.CreateScope())
        {
            var scopeServiceProvider = serviceScope.ServiceProvider;
            using (var context = scopeServiceProvider.GetService<T>())
            {
                if (context == null)
                    throw new InvalidOperationException("Unable to resolve database context");

                var logger = scopeServiceProvider.GetRequiredService<ILogger<T>>();

                if (context.IsExistingDatabase())
                {
                    logger.LogTrace("Database exists");

                    if (context.AnyMigrationsRewritten())
                    {
                        logger.LogWarning("Recreating database");

                        context.Database.EnsureDeleted();
                    }

                    if (!context.AllMigrationsApplied())
                    {
                        logger.LogInformation("Migrating database");

                        context.Database.Migrate();
                    }
                }
                else
                {
                    logger.LogInformation("Creating database");

                    context.Database.Migrate();
                }
            }

            // Seed.EnsureSeedData(scopeServiceProvider);
        }
    }
}
