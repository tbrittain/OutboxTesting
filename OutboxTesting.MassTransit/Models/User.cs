namespace OutboxTesting.MassTransit.Models;

public class User
{
    public HashedId Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<Post> Posts { get; set; } = [];
}

public class Post
{
    public HashedId Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
}