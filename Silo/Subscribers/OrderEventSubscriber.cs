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
    private IConnection? _connection;
    private IModel? _channel;
    private readonly ILogger<OrderEventSubscriber> _logger;

    public OrderEventSubscriber(IGrainFactory grainFactory, ILogger<OrderEventSubscriber> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    // Setup RabbitMQ connection and channel
    private void InitializeRabbitMq()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: Constants.RabbitmqOrderCreated,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueDeclare(queue: Constants.RabbitmqOrderDeleted,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("OrderEventSubscriber initialized and queues declared.");
    }

    private void StartConsumers()
    {
        CreateConsumer(Constants.RabbitmqOrderCreated, ProcessOrderCreatedMessage);
        CreateConsumer(Constants.RabbitmqOrderDeleted, ProcessOrderDeletedMessage);
    }

    private void CreateConsumer(string queueName, Func<string, Task> processMessage)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message from {Queue}: {Message}", queueName, message);

            try
            {
                await processMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {Queue}: {Message}", queueName, message);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
    }

    // Process the Order Created message
    private async Task ProcessOrderCreatedMessage(string message)
    {
        var order = DeserializeMessage<AddOrderRequest>(message);

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

    // Process the Order Deleted message
    private async Task ProcessOrderDeletedMessage(string message)
    {
        var changeOrderStatusRequest = DeserializeMessage<ChangeOrderStatusRequest>(message);

        if (changeOrderStatusRequest != null)
        {
            _logger.LogInformation("Processing order deletion: {OrderId}", changeOrderStatusRequest.IdOrder);
            var orderGrain = _grainFactory.GetGrain<IOrderGrain>(changeOrderStatusRequest.IdOrder.ToString());
            await orderGrain.UpdateStatus(changeOrderStatusRequest.NewOrderState.ToString());
            _logger.LogInformation("Order {OrderId} status updated to {NewStatus}.", changeOrderStatusRequest.IdOrder, changeOrderStatusRequest.NewOrderState);
        }
        else
        {
            _logger.LogWarning("Received invalid change order status request in {Queue}", Constants.RabbitmqOrderDeleted);
        }
    }

    // Deserialize the message
    private T? DeserializeMessage<T>(string message)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message: {Message}", message);
            return default;
        }
    }

    // Main execution loop
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        InitializeRabbitMq();
        StartConsumers();
        return Task.CompletedTask;
    }


    public override void Dispose()
    {
        if (_channel?.IsOpen == true)
        {
            _channel.Close();
        }

        if (_connection?.IsOpen == true)
        {
            _connection.Close();
        }

        base.Dispose();
    }
}