/*using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Abstractions;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .UseOrleansClient(client => { client.UseLocalhostClustering(); })
    .ConfigureLogging(logging => logging.AddConsole())
    .UseConsoleLifetime();

using IHost host = builder.Build();
await host.StartAsync();

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();

int customerCount = 5;
int riderCount = 3;
int ordersPerCustomer = 3;

List<string> productCatalog = new List<string> { "pizza", "pasta", "hamburger", "sushi", "insalata", "poke" };
List<ICustomerGrain> customers = new List<ICustomerGrain>();
List<IRiderGrain> riders = new List<IRiderGrain>();

for (int i = 1; i <= customerCount; i++)
{
    var customer = client.GetGrain<ICustomerGrain>($"customer{i}");
    customers.Add(customer);
    Console.WriteLine($"Customer {i} created.");
}

for (int i = 1; i <= riderCount; i++)
{
    var rider = client.GetGrain<IRiderGrain>($"rider{i}");
    riders.Add(rider);
    Console.WriteLine($"Rider {i} created.");
    await rider.SetWorking(true);
    await rider.SetAvailable(true);
}

Random random = new Random();
List<string> orderIds = new List<string>();

foreach (var customer in customers)
{
    for (int j = 1; j <= ordersPerCustomer; j++)
    {
        var orderItems = productCatalog.OrderBy(x => random.Next()).Take(3).ToList();

        // Simulate order creation
        var orderId = await customer.CreateOrder(orderItems);
        orderIds.Add(orderId);
        Console.WriteLine(
            $"Customer {customer.GetPrimaryKeyString()} created order {orderId} with items: {string.Join(", ", orderItems)}");

        // Randomly assign a rider to the order
        var randomRider = riders[random.Next(riders.Count)];
        var orderGrain = client.GetGrain<IOrderGrain>(orderId);
        try
        {
            await orderGrain.AssignToRider(randomRider.GetPrimaryKeyString());
            Console.WriteLine($"Order {orderId} assigned to Rider {randomRider.GetPrimaryKeyString()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to assign order {orderId}: {ex.Message}");
        }
    }
}

foreach (var orderId in orderIds)
{
    var deliveryGrain = client.GetGrain<IDeliveryGrain>(orderId);
    await Task.Delay(random.Next(10000, 30000)); // Simulate random delay for delivery

    await deliveryGrain.CompleteDelivery();
    Console.WriteLine($"Order {orderId} delivered.");
}

foreach (var customer in customers)
{
    foreach (var orderId in orderIds)
    {
        //var status = await customer.GetOrderStatus(orderId);
        //Console.WriteLine($"Order {orderId} status: {status}");
    }
}

Console.ReadKey();


await host.StopAsync();*/