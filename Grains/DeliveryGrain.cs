using Abstractions;
using distributedDeliveryBackend;

namespace Grains;

public class DeliveryGrain: Grain, IDeliveryGrain
{
    private string _orderId;
    private string _riderId;

    /*private readonly OrderEventPublisher _publisher;

    public DeliveryGrain(OrderEventPublisher orderEventPublisher)
    {
        _publisher = orderEventPublisher;
    }*/
        
    public Task StartDelivery(string orderId, string riderId)
    {
        _orderId = orderId;
        _riderId = riderId;
        // Start delivery logic here, e.g., send a notification
        return Task.CompletedTask;
    }

    public async Task CompleteDelivery()
    {
        //_publisher.PublishOrderDeliveredEvent(_orderId);
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(_orderId);
        await orderGrain.UpdateStatus("Delivered");

        var riderGrain = GrainFactory.GetGrain<IRiderGrain>(_riderId);
        await riderGrain.CompleteOrder();
    }
}