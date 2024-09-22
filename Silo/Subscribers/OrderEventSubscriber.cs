using System.Text;
using Abstractions;
using distributedDeliveryBackend.Dto;
using distributedDeliveryBackend.Dto.Request;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Constants = distributedDeliveryBackend.Utils.Constants;

namespace Silo.Subscribers;

public class OrderEventSubscriber : BackgroundService
{
    private readonly IGrainFactory _grainFactory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<OrderEventSubscriber> _logger;

    public OrderEventSubscriber(IGrainFactory grainFactory, ILogger<OrderEventSubscriber> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Queue for created orders
        _channel.QueueDeclare(queue: Constants.RabbitmqOrderCreated,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Queue for deleted orders
        _channel.QueueDeclare(queue: Constants.RabbitmqOrderDeleted,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        
        _logger.LogInformation("OrderEventSubscriber initialized and queues declared.");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consumer for created orders
        var orderCreatedConsumer = new EventingBasicConsumer(_channel);
        orderCreatedConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message from {Queue}: {Message}", Constants.RabbitmqOrderCreated, message);

            try
            {
                var order = JsonConvert.DeserializeObject<AddOrderRequest>(message);
                if (order != null)
                {
                    _logger.LogInformation("Processing order created: {OrderId}", order.IdOrder);
                    var deliveryGrain = _grainFactory.GetGrain<IDeliveryGrain>(order.IdOrder);
                    await deliveryGrain.StartDelivery(order.IdOrder!);
                    _logger.LogInformation("Order {OrderId} passed to the correct grain for delivery.", order.IdOrder);
                }
                else
                {
                    _logger.LogWarning("Received invalid or null order in {Queue}", Constants.RabbitmqOrderCreated);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing message from {Queue}: {Message}", Constants.RabbitmqOrderCreated, message);
            }
        };

        _channel.BasicConsume(queue: Constants.RabbitmqOrderCreated,
            autoAck: true,
            consumer: orderCreatedConsumer);

        // Consumer for deleted orders
        var orderDeletedConsumer = new EventingBasicConsumer(_channel);
        orderDeletedConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message from {Queue}: {Message}", Constants.RabbitmqOrderDeleted, message);

            try
            {
                var changeOrderStatusRequest = JsonConvert.DeserializeObject<ChangeOrderStatusRequest>(message);
                if (changeOrderStatusRequest != null)
                {
                    var grain = _grainFactory.GetGrain<IOrderGrain>(changeOrderStatusRequest.IdOrder.ToString());
                    await grain.UpdateStatus(changeOrderStatusRequest.NewOrderState.ToString());
                    _logger.LogInformation("Order {OrderId} status updated to {NewStatus}.", changeOrderStatusRequest.IdOrder, changeOrderStatusRequest.NewOrderState);

                    _channel.BasicConsume(queue: Constants.RabbitmqOrderDeleted,
                        autoAck: true,
                        consumer: orderDeletedConsumer);
                }
                else
                {
                    _logger.LogWarning("Received invalid change order status request in {Queue}", Constants.RabbitmqOrderDeleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing message from {Queue}: {Message}", Constants.RabbitmqOrderDeleted, message);
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