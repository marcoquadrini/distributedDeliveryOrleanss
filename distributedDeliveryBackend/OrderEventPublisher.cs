using RabbitMQ.Client;
using System.Text;
using distributedDeliveryBackend.Dto;
using distributedDeliveryBackend.Utils;
using Newtonsoft.Json;
using Constants = distributedDeliveryBackend.Utils.Constants;

public class OrderEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public OrderEventPublisher()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: Constants.rabbitmq_order_created,
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
            routingKey: Constants.rabbitmq_order_created,
            basicProperties: null,
            body: body);
    }
    
    public void PublishOrderDeletedEvent(int idOrder)
    {
        var body = Encoding.UTF8.GetBytes(idOrder.ToString());
        _channel.BasicPublish(exchange: "",
            routingKey: "order_deleted",
            basicProperties: null,
            body: body);
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}