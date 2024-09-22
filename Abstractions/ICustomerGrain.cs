namespace Abstractions;

/**
 * Represents a customer that can place orders
 */
public interface ICustomerGrain : IGrainWithStringKey
{
    Task<string> CreateOrder(List<string> productIds);
    
}