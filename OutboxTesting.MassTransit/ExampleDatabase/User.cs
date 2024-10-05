using Microsoft.EntityFrameworkCore;
using Bogus;

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
    private static readonly Faker<User> UserConfiguration = new Faker<User>()
        .RuleFor(u => u.FirstName, f => f.Person.FirstName)
        .RuleFor(u => u.LastName, f => f.Person.LastName)
        .RuleFor(u => u.Email, f => f.Person.Email);

    public static User GenerateUser()
    {
        return UserConfiguration.Generate();
    }

    public static void ConfigureUser(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id).IsClustered();
            
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