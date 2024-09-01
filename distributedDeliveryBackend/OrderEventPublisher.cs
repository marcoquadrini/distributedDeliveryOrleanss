using RabbitMQ.Client;
using System.Text;
using distributedDeliveryBackend.Dto;
using Newtonsoft.Json;

public class OrderEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public OrderEventPublisher()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "order_queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void PublishOrderCreatedEvent(OrderDb order)
    {
        var message = JsonConvert.SerializeObject(order);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "",
            routingKey: "order_queue",
            basicProperties: null,
            body: body);
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}