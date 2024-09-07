using System.Text;
using distributedDeliveryBackend.Dto;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Constants = distributedDeliveryBackend.Utils.Constants;

namespace distributedDeliveryBackend;

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

    public void PublishOrderDeletedEvent(ChangeOrderStatusRequest request)
    {
        var message = JsonConvert.SerializeObject(request);
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "",
            routingKey: "order_deleted",
            basicProperties: null,
            body: body);
    }

    public void PublishOrderDeliveredEvent(string idOrder)
    {
        var body = Encoding.UTF8.GetBytes($"Order Delivered Notification: Your order {idOrder} was succeasfully delivered");
        _channel.BasicPublish(exchange: "",
            routingKey: "order_delivered",
            basicProperties: null,
            body: body);
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}