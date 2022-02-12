namespace Microsoft.EntityFrameworkCore;

static class DbContextExtensions
{
    public static bool IsExistingDatabase(this DbContext context)
    {
        var databaseCreator = context.Database.GetService<IDatabaseCreator>();
        var relationalCreator = (databaseCreator as RelationalDatabaseCreator);
        return relationalCreator.Exists();
    }

    public static bool AllMigrationsApplied(this DbContext context)
    {
        var applied = context.GetService<IHistoryRepository>()
            .GetAppliedMigrations()
            .Select(m => m.MigrationId);

        var total = context.GetService<IMigrationsAssembly>()
            .Migrations
            .Select(m => m.Key);

        return !total.Except(applied).Any();
    }

    public static bool AnyMigrationsRewritten(this DbContext context)
    {
        var applied = context.GetService<IHistoryRepository>()
            .GetAppliedMigrations()
            .Select(m => m.MigrationId);

        var total = context.GetService<IMigrationsAssembly>()
            .Migrations
            .Select(m => m.Key);

        return applied.Except(total).Any();
    }
}
