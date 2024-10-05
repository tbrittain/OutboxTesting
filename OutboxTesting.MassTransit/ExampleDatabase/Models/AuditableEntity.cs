namespace OutboxTesting.MassTransit.ExampleDatabase.Models;

public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}