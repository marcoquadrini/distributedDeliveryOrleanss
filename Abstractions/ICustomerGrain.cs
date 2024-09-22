namespace Abstractions;

/// <summary>
/// Represents a customer that can place orders
/// </summary>
public interface ICustomerGrain : IGrainWithStringKey
{
    Task<string> CreateOrder(List<string> products);
    
}