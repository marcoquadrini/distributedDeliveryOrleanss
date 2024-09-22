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

public class RiderEventSubscriber : BackgroundService
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<RiderEventSubscriber> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RiderEventSubscriber(IGrainFactory grainFactory, ILogger<RiderEventSubscriber> logger)
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

        _channel.QueueDeclare(queue: Constants.RabbitmqSetWorkingRider,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("RiderEventSubscriber initialized and queue declared.");
    }

    private void StartConsumer()
    {
        CreateConsumer(Constants.RabbitmqSetWorkingRider, ProcessSetWorkingRiderRequest);
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

    // Process the Set Working Rider message
    private async Task ProcessSetWorkingRiderRequest(string message)
    {
        var setWorkingRiderRequest = DeserializeMessage<SetWorkingRiderRequest>(message);

        if (setWorkingRiderRequest == null)
        {
            _logger.LogWarning("Received null or invalid SetWorkingRiderRequest from {Queue}", Constants.RabbitmqSetWorkingRider);
            return;
        }

        _logger.LogInformation("Processing SetWorkingRiderRequest for RiderId: {RiderId}", setWorkingRiderRequest.RiderId);

        var riderGrain = _grainFactory.GetGrain<IRiderGrain>(setWorkingRiderRequest.RiderId);
        await riderGrain.SetWorking(setWorkingRiderRequest.IsWorking);
        
        _logger.LogInformation("Rider {RiderId} working status updated to: {IsWorking}", setWorkingRiderRequest.RiderId, setWorkingRiderRequest.IsWorking);
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

    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        InitializeRabbitMq();
        StartConsumer();
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