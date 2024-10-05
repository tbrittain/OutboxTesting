using MassTransit;
using OutboxTesting.MassTransit.Services;

namespace OutboxTesting.MassTransit.PubSub;

public record GenerateMultiUser(int NumUsers);

// ReSharper disable once UnusedType.Global
public class GenerateMultiUserConsumer(
    ILogger<GenerateMultiUserConsumer> logger, UserRepository userRepository)
    : IConsumer<GenerateMultiUser>
{
    public async Task Consume(ConsumeContext<GenerateMultiUser> context)
    {
        var numUsers = context.Message.NumUsers;
        for (var i = 0; i < numUsers; i++)
        {
            await userRepository.CreateUser();
        }

        logger.LogInformation("Generated {NumUsers} users", numUsers);
    }
}