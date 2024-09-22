using System.Text;
using distributedDeliveryBackend.Dto;
using distributedDeliveryBackend.Dto.Request;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Constants = distributedDeliveryBackend.Utils.Constants;

namespace distributedDeliveryBackend.Publishers;

public class OrderEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<OrderEventPublisher> _logger;

    public OrderEventPublisher(ILogger<OrderEventPublisher> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };
        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            //Queue to dispatch order created events
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

            _logger.LogInformation("OrderEventPublisher initialized and queue declared.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection or channel.");
            throw;
        }
    }

    public void PublishOrderCreatedEvent(AddOrderRequest order)
    {
        try
        {
            var message = JsonConvert.SerializeObject(order);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                routingKey: Constants.RabbitmqOrderCreated,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Order created event published for OrderId: {OrderId}", order.IdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish order created event for OrderId: {OrderId}", order.IdOrder);
        }
    }

    public void PublishOrderDeletedEvent(ChangeOrderStatusRequest request)
    {
        try
        {
            var message = JsonConvert.SerializeObject(request);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                routingKey: "order_deleted",
                basicProperties: null,
                body: body);

            _logger.LogInformation("Order deleted event published for OrderId: {OrderId}", request.IdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish order deleted event for OrderId: {OrderId}", request.IdOrder);
        }
    }
    
    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}