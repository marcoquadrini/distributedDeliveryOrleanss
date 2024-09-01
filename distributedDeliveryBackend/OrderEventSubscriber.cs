using System.Text;
using Abstractions;
using distributedDeliveryBackend.Dto;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class OrderEventSubscriber : BackgroundService
{
    private readonly IGrainFactory _grainFactory;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public OrderEventSubscriber(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "order_queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = JsonConvert.DeserializeObject<OrderDb>(message);

            // Log to verify execution
            Console.WriteLine("Received message and now passing it to the correct grain");

            if (order == null) return;

            var grain = _grainFactory.GetGrain<IOrderAssignmentGrain>(order.Id);
            await grain.HandleOrderCreatedEvent(order.Id);
        };

        _channel.BasicConsume(queue: "order_queue",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}