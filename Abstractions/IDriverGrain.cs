using Microsoft.CodeAnalysis;
using Orleans;

namespace Abstractions;

public interface IDriverGrain : IGrainWithStringKey
{
    Task UpdateLocation(Location location);

    Task <Location> GetLocation(Location location);

    Task AssignOrder(String orderKey);

    Task<String> GetName();
}


