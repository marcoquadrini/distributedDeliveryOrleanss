namespace Abstractions;

/// <summary>
/// Represents a delivery of an order to a given customer, managed by a given rider
/// </summary>
public interface IDeliveryGrain : IGrainWithStringKey
{
    Task StartDelivery(string orderId);

    Task CompleteDelivery();

    Task<string?> ChooseRider();
    
    Task<string?> ContinueDelivery();

    Task<string> GetOrderId();
}