namespace Scullery.EntityFrameworkCore;

public class SculleryContext : DbContext
{
    public SculleryContext(DbContextOptions<SculleryContext> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //
        // Keys
        //

        modelBuilder.Entity<Job>()
            .HasKey(x => x.Id);
    }
}
