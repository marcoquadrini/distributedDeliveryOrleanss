using Microsoft.CodeAnalysis;
using Orleans;

namespace Abstractions;

public interface IRiderGrain : IGrainWithStringKey
{
    Task<bool> SetInfo(string name, string lastName, bool isWorking);
    
    Task AssignOrder(string orderId);

    Task<String> GetName();
    
    Task<bool> IsAvailable();
    
    Task<bool> IsWorking();
    
    Task<bool> SetWorking(bool working);
    
    Task SetAvailable(bool available);

    Task CompleteOrder();
}


