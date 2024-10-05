using Microsoft.EntityFrameworkCore;

namespace OutboxTesting.MassTransit.ExampleDatabase;

public class User : AuditableEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    
    public virtual ICollection<Post> Posts { get; set; }
    public virtual ICollection<User> Following { get; set; }
    public virtual ICollection<User> Followers { get; set; }
    public virtual ICollection<Post> LikedPosts { get; set; }
}

public static class UserExtensions
{
    public static void ConfigureUser(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Following)
                .WithMany(u => u.Followers)
                .UsingEntity(j => j.ToTable("UserFollows"));

            entity.HasMany(u => u.LikedPosts)
                .WithMany(p => p.LikedBy)
                .UsingEntity(j => j.ToTable("PostLikes"));
        });
    }
}