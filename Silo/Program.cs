using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
            })
            .ConfigureLogging(logging => { logging.AddConsole(); })
            .Build();

        await host.RunAsync();
    }
}

