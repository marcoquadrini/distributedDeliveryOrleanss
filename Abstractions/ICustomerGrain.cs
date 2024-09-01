namespace Abstractions;

public interface ICustomerGrain : IGrainWithStringKey
{
    Task<string> CreateOrder(List<string> productIds);
    
    Task<string> GetOrderStatus(string orderId);
    
}