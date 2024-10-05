using Bogus;
using Microsoft.EntityFrameworkCore;

namespace OutboxTesting.MassTransit.ExampleDatabase;

public class Post : AuditableEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; }
    
    public virtual ICollection<User> LikedBy { get; set; }
}

public static class PostExtensions
{
    private static readonly Faker<Post> UserConfiguration = new Faker<Post>()
        .RuleFor(p => p.Title, f => f.Lorem.Sentence())
        .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(3));

    public static IEnumerable<Post> GeneratePosts(int userId)
    {
        var post = UserConfiguration.Generate();
        post.UserId = userId;

        yield return post;
    }

    public static void ConfigurePost(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(u => u.Id).IsClustered();

            entity.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.LikedBy)
                .WithMany(u => u.LikedPosts)
                .UsingEntity(j => j.ToTable("PostLikes"));
        });

        modelBuilder.Entity<Post>()
            .HasQueryFilter(r => r.DeletedAt != null);

        modelBuilder.Entity<Post>()
            .HasIndex(r => r.DeletedAt)
            .HasFilter("DeletedAt IS NOT NULL");
    }
}