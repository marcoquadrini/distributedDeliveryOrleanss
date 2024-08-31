using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using System.Diagnostics;
using System.Net;


var host = Host.CreateDefaultBuilder()
    .UseOrleans((ctx, silo) =>
    {
        silo.UseLocalhostClustering();
        silo.UseDashboard();
    }
    )
    .Build();

await host.StartAsync();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

await host.StopAsync();
