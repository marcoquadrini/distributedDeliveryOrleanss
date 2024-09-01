using Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains;

public class RiderGrain : Grain, IRiderGrain
{
    private readonly ILogger logger;

    public RiderGrain(ILogger<RiderGrain> logger)
    {
        this.logger = logger;
    }

    public Task AssignOrder(string orderKey)
    {
        throw new NotImplementedException();
    }

    public Task<Location> GetLocation(Location location)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetName()
    {
        return Task.FromResult("NOME PROVA");
    }

    public Task UpdateLocation(Location location)
    {
        throw new NotImplementedException();
    }
}
