using Orleans;

namespace Abstractions;

public interface IOrderAssignmentGrain : IGrainWithIntegerKey
{
    Task HandleOrderCreatedEvent(int orderId);
}