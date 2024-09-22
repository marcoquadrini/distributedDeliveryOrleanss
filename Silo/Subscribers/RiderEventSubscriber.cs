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
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RiderEventSubscriber> _logger;

    public RiderEventSubscriber(IGrainFactory grainFactory, ILogger<RiderEventSubscriber> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        var setWorkingRider = new EventingBasicConsumer(_channel);
        setWorkingRider.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message from {Queue}: {Message}", Constants.RabbitmqSetWorkingRider, message);

            try
            {
                var setWorkingRiderRequest = JsonConvert.DeserializeObject<SetWorkingRiderRequest>(message);
                if (setWorkingRiderRequest == null)
                {
                    _logger.LogWarning("Received null or invalid SetWorkingRiderRequest from {Queue}", Constants.RabbitmqSetWorkingRider);
                    return;
                }

                _logger.LogInformation("Processing SetWorkingRiderRequest for RiderId: {RiderId}", setWorkingRiderRequest.RiderId);

                var rider = _grainFactory.GetGrain<IRiderGrain>(setWorkingRiderRequest.RiderId);
                await rider.SetWorking(setWorkingRiderRequest.IsWorking);
                
                _logger.LogInformation("Rider {RiderId} working status updated to: {IsWorking}", setWorkingRiderRequest.RiderId, setWorkingRiderRequest.IsWorking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing SetWorkingRiderRequest from {Queue}", Constants.RabbitmqSetWorkingRider);
            }
        };
        
        _channel.BasicConsume(queue: Constants.RabbitmqSetWorkingRider,
            autoAck: true,
            consumer: setWorkingRider);
        
        return Task.CompletedTask;
    }


    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}