namespace Abstractions;

public interface IDeliveryGrain : IGrainWithStringKey
{
    Task StartDelivery(string orderId);

    Task CompleteDelivery();

    Task<bool> ChooseRider();
    
    Task ContinueDelivery();

}