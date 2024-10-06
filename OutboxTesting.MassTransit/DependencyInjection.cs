using System.Diagnostics;
using MassTransit;
using OutboxTesting.MassTransit.ExampleDatabase;
using OutboxTesting.MassTransit.PubSub;
using OutboxTesting.MassTransit.Services;

namespace OutboxTesting.MassTransit;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");
        Trace.Assert(!string.IsNullOrWhiteSpace(connectionString), "Connection string is null or empty");
        services.AddSqlServer<ExampleDbContext>(connectionString);
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<GenerateMultiUserConsumer>();
            
            // https://masstransit.io/documentation/configuration/middleware/outbox#configuration
            x.AddEntityFrameworkOutbox<ExampleDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, configurator) =>
            {
                var rabbitMqSection = configuration.GetSection("RabbitMq");
                var host = rabbitMqSection.GetValue<string>("Host");
                Trace.Assert(!string.IsNullOrWhiteSpace(host), "Host is null or empty");

                var username = rabbitMqSection.GetValue<string>("Username");
                Trace.Assert(!string.IsNullOrWhiteSpace(username), "Username is null or empty");
                
                var password = rabbitMqSection.GetValue<string>("Password");
                Trace.Assert(!string.IsNullOrWhiteSpace(password), "Password is null or empty");
                
                configurator.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                configurator.UseMessageRetry(r => r.Intervals(100, 500, 1000, 1500));
                configurator.ConfigureEndpoints(context);
            });

            x.AddConfigureEndpointsCallback((context, name, cfg) =>
            {
                cfg.UseEntityFrameworkOutbox<ExampleDbContext>(context);
            });
        });

        return services;
    }
}