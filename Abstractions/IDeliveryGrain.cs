namespace Abstractions;

public interface IDeliveryGrain : IGrainWithStringKey
{
    Task StartDelivery(string orderId);

    Task CompleteDelivery();

    Task ChooseRider();

}