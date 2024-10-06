using Microsoft.EntityFrameworkCore;
using OutboxTesting.MassTransit.ExampleDatabase;
using OutboxTesting.MassTransit.ExampleDatabase.Models;
using OutboxTesting.MassTransit.Models;
using User = OutboxTesting.MassTransit.Models.User;

namespace OutboxTesting.MassTransit.Services;

public interface IUserRepository
{
    Task<User> CreateUser();
    Task<User?> GetUser(int id);
    Task<PaginatedResult<User>> GetUsers(int pageNumber, int pageSize);
    Task<bool> DeleteUser(int id);
}

public class UserRepository(ExampleDbContext exampleDbContext) : IUserRepository
{
    public async Task<User> CreateUser()
    {
        var user = UserExtensions.GenerateUser();
        exampleDbContext.Users.Add(user);
        await exampleDbContext.SaveChangesAsync();

        return new User
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public async Task<User?> GetUser(int id)
    {
        var user = await exampleDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return null;
        }

        return new User
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public async Task<PaginatedResult<User>> GetUsers(int pageNumber, int pageSize)
    {
        var queryable = exampleDbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new User
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            });
        
        var users = await queryable.ToListAsync();

        var totalUsers = await exampleDbContext.Users.CountAsync();
        return new PaginatedResult<User>(users, pageNumber, pageSize, totalUsers);
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