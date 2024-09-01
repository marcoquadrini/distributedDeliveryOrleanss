﻿using System.Net;
using Grains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;


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
            })
            .ConfigureLogging(logging => { logging.AddConsole(); })
            .Build();

        await host.RunAsync();
    }
}

