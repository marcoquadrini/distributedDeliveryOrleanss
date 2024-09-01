using Orleans;

namespace Abstractions;

public interface IOrderAssignmentGrain : IGrainWithIntegerKey
{
    Task HandleOrderCreatedEvent(int orderId);
    
    Task StartDelivery(string orderId, string riderId);
    
    Task CompleteDelivery();
}