using OutboxTesting.MassTransit.ExampleDatabase;

namespace OutboxTesting.MassTransit.Services;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = new());
}

public class UnitOfWork(ExampleDbContext exampleDbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        await exampleDbContext.SaveChangesAsync(cancellationToken);
    }
}