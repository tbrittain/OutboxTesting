using MassTransit;
using OutboxTesting.MassTransit.ExampleDatabase;

namespace OutboxTesting.MassTransit;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
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