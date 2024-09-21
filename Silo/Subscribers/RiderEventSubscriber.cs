using System.Text;
using Abstractions;
using distributedDeliveryBackend.Dto;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using distributedDeliveryBackend.Utils;
using Grains.States;
using Microsoft.Extensions.Hosting;
using Constants = distributedDeliveryBackend.Utils.Constants;

public class RiderEventSubscriber : BackgroundService
{
    private readonly IGrainFactory _grainFactory;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RiderEventSubscriber(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        
        _channel.QueueDeclare(queue: Constants.RabbitmqSetWorkingRider,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        var setWorkingRider = new EventingBasicConsumer(_channel);
        setWorkingRider.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var setWorkingRiderRequest = JsonConvert.DeserializeObject<SetWorkingRiderRequest>(message);
            if(setWorkingRiderRequest == null) return;
            var rider = _grainFactory.GetGrain<IRiderGrain>(setWorkingRiderRequest.RiderId);
            await rider.SetWorking(setWorkingRiderRequest.IsWorking);
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