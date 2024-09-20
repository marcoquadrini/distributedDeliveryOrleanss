namespace Abstractions;

public interface IDeliveryGrain : IGrainWithStringKey
{
    Task StartDelivery(string orderId, string riderId);

    Task CompleteDelivery();
    
}