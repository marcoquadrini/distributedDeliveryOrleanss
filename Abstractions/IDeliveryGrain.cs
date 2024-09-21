namespace Abstractions;

public interface IDeliveryGrain : IGrainWithStringKey
{
    Task StartDelivery(string orderId);

    Task CompleteDelivery();

    Task<string?> ChooseRider();
    
    Task<string?> ContinueDelivery();

}