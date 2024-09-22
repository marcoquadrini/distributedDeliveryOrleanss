using System.Text;
using distributedDeliveryBackend.Dto.Request;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Constants = distributedDeliveryBackend.Utils.Constants;

namespace distributedDeliveryBackend.Publishers;

public class RiderEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RiderEventPublisher> _logger;

    public RiderEventPublisher(ILogger<RiderEventPublisher> logger)
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

            // Queue to send new riders messages
            _channel.QueueDeclare(queue: Constants.RabbitmqNewRider,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Queue to send rider working events
            _channel.QueueDeclare(queue: Constants.RabbitmqSetWorkingRider,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("RiderEventPublisher initialized and queues declared.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection or declare queues.");
            throw;
        }
    }

    public void PublishNewRiderEvent(AddRiderRequest rider)
    {
        try
        {
            var message = JsonConvert.SerializeObject(rider);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                routingKey: Constants.RabbitmqNewRider,
                basicProperties: null,
                body: body);

            _logger.LogInformation("New rider event published");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish new rider event");
        }
    }
    
    public void PublishSetWorkingRider(SetWorkingRiderRequest rider)
    {
        try
        {
            var message = JsonConvert.SerializeObject(rider);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                routingKey: Constants.RabbitmqSetWorkingRider,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Set working rider event published for RiderId: {RiderId}, Working: {IsWorking}", rider.RiderId, rider.IsWorking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish set working rider event for RiderId: {RiderId}", rider.RiderId);
        }
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}