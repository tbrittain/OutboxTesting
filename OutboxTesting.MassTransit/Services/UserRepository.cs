using OutboxTesting.MassTransit.ExampleDatabase;
using OutboxTesting.MassTransit.Models;

namespace OutboxTesting.MassTransit.Services;

public interface IUserRepository
{
    Task<Models.User> CreateUser();
    Task<Models.User?> GetUser(int id);
    Task<bool> DeleteUser(int id);
}

public class UserRepository(ExampleDbContext exampleDbContext) : IUserRepository
{
    public async Task<Models.User> CreateUser()
    {
        var user = UserExtensions.GenerateUser();
        exampleDbContext.Users.Add(user);
        await exampleDbContext.SaveChangesAsync();

        return new Models.User
        {
            Id = new HashedId(user.Id),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public async Task<Models.User?> GetUser(int id)
    {
        var user = await exampleDbContext.Users.FindAsync(id);

        if (user is null)
        {
            return null;
        }

        return new Models.User
        {
            Id = new HashedId(user.Id),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await exampleDbContext.Users.FindAsync(id);

        if (user is null)
        {
            return false;
        }

        exampleDbContext.Users.Remove(user);
        var ok = await exampleDbContext.SaveChangesAsync();
        return ok > 0;
    }
}