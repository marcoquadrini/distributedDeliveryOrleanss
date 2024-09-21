using System.Text;
using distributedDeliveryBackend.Dto;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Constants = distributedDeliveryBackend.Utils.Constants;

namespace distributedDeliveryBackend;

public class RiderEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RiderEventPublisher()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: Constants.RabbitmqNewRider,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void PublishNewRiderEvent(AddRiderRequest rider)
    {
        var message = JsonConvert.SerializeObject(rider);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "",
            routingKey: Constants.RabbitmqNewRider,
            basicProperties: null,
            body: body);
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}