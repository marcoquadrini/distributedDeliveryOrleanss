using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Abstractions;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client =>
    {
        client.UseLocalhostClustering();
    })
    .ConfigureLogging(logging => logging.AddConsole())
    .UseConsoleLifetime();

using IHost host = builder.Build();
await host.StartAsync();

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();

IRiderGrain driver = client.GetGrain<IRiderGrain>("prova");
string response = await driver.GetName();

Console.WriteLine($"""
    {response}

    Press any key to exit...
    """);

Console.ReadKey();

await host.StopAsync();