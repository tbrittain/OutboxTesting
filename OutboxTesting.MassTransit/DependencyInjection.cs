using System.Diagnostics;
using MassTransit;
using OutboxTesting.MassTransit.ExampleDatabase;
using OutboxTesting.MassTransit.Services;

namespace OutboxTesting.MassTransit;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");
        Trace.Assert(!string.IsNullOrWhiteSpace(connectionString), "Connection string is null or empty");
        services.AddSqlServer<ExampleDbContext>(connectionString);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        services.AddMassTransit(x =>
        {
            // https://masstransit.io/documentation/configuration/middleware/outbox#configuration
            x.AddEntityFrameworkOutbox<ExampleDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            // TODO: use rabbitmq with docker
            x.UsingInMemory((context, configurator) =>
            {
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