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

        
        _channel.QueueDeclare(queue: Constants.RabbitmqNewRider,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var riderConsumer = new EventingBasicConsumer(_channel);
        riderConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var riderStateJson = JsonConvert.DeserializeObject<AddRiderRequest>(message);
            if (riderStateJson != null)
            {
                var riderGrain = _grainFactory.GetGrain<IRiderGrain>(Guid.NewGuid().ToString());
                riderGrain.SetInfo(riderStateJson.Name, riderStateJson.Lastname, riderStateJson.IsWorking);
            }
            
        };
        
        _channel.BasicConsume(queue: Constants.RabbitmqNewRider,
            autoAck: true,
            consumer: riderConsumer);
        return Task.CompletedTask;
    }


public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}