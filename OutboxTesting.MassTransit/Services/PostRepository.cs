using OutboxTesting.MassTransit.ExampleDatabase;
using OutboxTesting.MassTransit.ExampleDatabase.Models;
using Post = OutboxTesting.MassTransit.Models.Post;

namespace OutboxTesting.MassTransit.Services;

public interface IPostRepository
{
    Task<Post> CreatePost(int userId);
    Task<Post?> GetPost(int id);
    Task<bool> DeletePost(int id);
    Task<bool> LikePost(int id, int userId);
}

public class PostRepository(ExampleDbContext exampleDbContext, ILogger<PostRepository> logger) : IPostRepository
{
    public async Task<Post> CreatePost(int userId)
    {
        var post = PostExtensions
            .GeneratePosts(userId)
            .First();
        
        exampleDbContext.Posts.Add(post);
        await exampleDbContext.SaveChangesAsync();

        return new Post
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content
        };
    }

    public async Task<Post?> GetPost(int id)
    {
        var post = await exampleDbContext.Posts.FindAsync(id);
        if (post is null)
        {
            logger.LogWarning("Post not found when attempting to get: {Id}", id);
            return null;
        }

        return new Post
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content
        };
    }

    public async Task<bool> DeletePost(int id)
    {
        var post = await exampleDbContext.Posts.FindAsync(id);

        if (post is null)
        {
            logger.LogWarning("Post not found while attempting to delete: {Id}", id);
            return false;
        }

        exampleDbContext.Posts.Remove(post);
        var ok = await exampleDbContext.SaveChangesAsync();
        return ok > 0;
    }

    public async Task<bool> LikePost(int id, int userId)
    {
        var post = await exampleDbContext.Posts.FindAsync(id);
        var user = await exampleDbContext.Users.FindAsync(userId);

        if (post is null)
        {
            logger.LogWarning("Post not found when attempting to like: {Id}", id);
            return false;
        }

        if (user is null)
        {
            logger.LogWarning("User not found when attempting to like post: {Id}", id);
            return false;
        }

        post.LikedBy.Add(user);
        var ok = await exampleDbContext.SaveChangesAsync();
        return ok > 0;
    }
}