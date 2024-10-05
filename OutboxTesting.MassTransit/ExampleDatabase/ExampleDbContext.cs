using MassTransit;
using Microsoft.EntityFrameworkCore;
using OutboxTesting.MassTransit.ExampleDatabase.Models;

namespace OutboxTesting.MassTransit.ExampleDatabase;

public class ExampleDbContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Post> Posts { get; set; }
    
    public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureUser();
        modelBuilder.ConfigurePost();

        // The following are MassTransit-related entities
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        
        base.OnModelCreating(modelBuilder);
    }

    override public Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e is {Entity: AuditableEntity, State: EntityState.Added or EntityState.Modified});

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    ((AuditableEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    ((AuditableEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    ((AuditableEntity)entry.Entity).DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public static void ApplyMigrations(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
        dbContext.Database.Migrate();
    }
}