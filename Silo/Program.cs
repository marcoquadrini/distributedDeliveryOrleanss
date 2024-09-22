using Grains.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silo.Subscribers;
using StackExchange.Redis;

namespace Silo;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
                siloBuilder.AddRedisGrainStorageAsDefault(options =>
                {
                    options.ConfigurationOptions = new ConfigurationOptions();
                    options.ConfigurationOptions.EndPoints.Add("localhost", 6379);
                });
                siloBuilder.Services.AddHostedService<OrderEventSubscriber>();
                siloBuilder.Services.AddHostedService<RiderEventSubscriber>();
                siloBuilder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var configuration = ConfigurationOptions.Parse("localhost:6379");
                    return ConnectionMultiplexer.Connect(configuration);
                });
                siloBuilder.Services.AddSingleton<RiderAvailabilityService>();
                siloBuilder.Services.AddSingleton<PendingDeliveriesService>();
            })
            .ConfigureLogging(logging => { logging.AddConsole(); })
            .Build();

        await host.RunAsync();
    }
}