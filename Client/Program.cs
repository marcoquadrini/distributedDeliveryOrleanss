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

// Simulating order creation
var customerGrain = client.GetGrain<ICustomerGrain>("customer123");
var orderId = await customerGrain.CreateOrder(new List<string>{"product1", "product2", "product3"});
Console.WriteLine($"Order {orderId} created");

// Simulating rider assignment
var riderGrain = client.GetGrain<IRiderGrain>("rider789");
var orderGrain = client.GetGrain<IOrderGrain>(orderId);

try
{
    await orderGrain.AssignToRider("rider789");
    Console.WriteLine($"Order {orderId} assigned to rider789");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to assign order: {ex.Message}");
}

// Simulating delivery completion
var deliveryGrain = client.GetGrain<IDeliveryGrain>(orderId);
await deliveryGrain.CompleteDelivery();
Console.WriteLine($"Order {orderId} delivered");

// Fetch order status
var status = await customerGrain.GetOrderStatus(orderId);
Console.WriteLine($"Order {orderId} status: {status}");

Console.WriteLine($"""
    {response}

    Press any key to exit...
    """);

Console.ReadKey();





await host.StopAsync();