using Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains;

public class DriverGrain : Grain, IDriverGrain
{
    private readonly ILogger logger;

    public DriverGrain(ILogger<DriverGrain> logger)
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
