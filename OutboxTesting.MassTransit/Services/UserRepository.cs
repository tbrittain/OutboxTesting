using OutboxTesting.MassTransit.ExampleDatabase;

namespace OutboxTesting.MassTransit.Services;

public class UserRepository(ExampleDbContext exampleDbContext)
{
    public async Task<Models.User> CreateUser()
    {
        var user = UserExtensions.GenerateUser();
        exampleDbContext.Users.Add(user);
        await exampleDbContext.SaveChangesAsync();

        return new Models.User
        {
            Id = user.Id,
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
            Id = user.Id,
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