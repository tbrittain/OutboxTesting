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
    public static void ConfigurePost(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.LikedBy)
                .WithMany(u => u.LikedPosts)
                .UsingEntity(j => j.ToTable("PostLikes"));
        });
    }
}