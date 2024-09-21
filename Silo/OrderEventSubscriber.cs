using System.Text;
using Abstractions;
using distributedDeliveryBackend.Dto;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using distributedDeliveryBackend.Utils;
using Microsoft.Extensions.Hosting;
using Constants = distributedDeliveryBackend.Utils.Constants;

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

        // First queue for created order
        _channel.QueueDeclare(queue: Constants.rabbitmq_order_created,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Second queue for deleted order
        _channel.QueueDeclare(queue: Constants.rabbitmq_order_deleted,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consumer first queue
        var orderCreatedConsumer = new EventingBasicConsumer(_channel);
        orderCreatedConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = JsonConvert.DeserializeObject<AddOrderRequest>(message);

            // Log per verificare l'esecuzione
            Console.WriteLine("Received order created message and now passing it to the correct grain");

            if (order == null) return;
            
        };

        _channel.BasicConsume(queue: Constants.rabbitmq_order_created,
            autoAck: true,
            consumer: orderCreatedConsumer);

        // Consumer second queue
        var orderDeletedConsumer = new EventingBasicConsumer(_channel);
        orderDeletedConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var changeOrderStatusRequest = JsonConvert.DeserializeObject<ChangeOrderStatusRequest>(message);
            if (changeOrderStatusRequest != null)
            {
                try
                {
                    var grain = _grainFactory.GetGrain<IOrderGrain>(changeOrderStatusRequest.idOrder.ToString());
                    await grain.UpdateStatus(changeOrderStatusRequest.newOrderState.ToString());
                    _channel.BasicConsume(queue: Constants.rabbitmq_order_deleted,
                        autoAck: true,
                        consumer: orderDeletedConsumer);
                    Console.WriteLine($"Order deleted id : {changeOrderStatusRequest.idOrder}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Order not valid: {e.Message}");
                }
            }
        };
        
        return Task.CompletedTask;
    }


public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}