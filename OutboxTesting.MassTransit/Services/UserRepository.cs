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
    Task<bool> FollowUser(int followerId, int toFollowId);
}

public class UserRepository(ExampleDbContext exampleDbContext, ILogger<UserRepository> logger) : IUserRepository
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
            logger.LogWarning("User not found when attempting to get: {Id}", id);
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
            logger.LogWarning("User not found when attempting to delete: {Id}", id);
            return false;
        }

        exampleDbContext.Users.Remove(user);
        var ok = await exampleDbContext.SaveChangesAsync();
        return ok > 0;
    }

    public async Task<bool> FollowUser(int followerId, int toFollowId)
    {
        var users = await exampleDbContext.Users
            .Where(u => u.Id == followerId || u.Id == toFollowId)
            .ToListAsync();

        if (users.Count != 2)
        {
            logger.LogWarning("One or both users not found when attempting to follow: {FollowerId}, {FolloweeId}", followerId, toFollowId);
            return false;
        }
        
        var follower = users.Single(u => u.Id == followerId);
        var toFollow = users.Single(u => u.Id == toFollowId);

        follower.Following.Add(toFollow);
        await exampleDbContext.SaveChangesAsync();
        
        return true;
    }
}