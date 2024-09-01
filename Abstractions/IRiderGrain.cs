using Microsoft.CodeAnalysis;
using Orleans;

namespace Abstractions;

public interface IRiderGrain : IGrainWithStringKey
{
    Task UpdateLocation(Location location);

    Task <Location> GetLocation(Location location);

    Task AssignOrder(string orderId);

    Task<String> GetName();
    
    Task<bool> IsAvailable();
    
    Task<bool> IsWorking();
    
    Task SetWorking(bool working);

    Task CompleteOrder();
}


