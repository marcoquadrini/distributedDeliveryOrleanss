using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            })
            .ConfigureLogging(logging => { logging.AddConsole(); })
            .Build();

        await host.RunAsync();
    }
}