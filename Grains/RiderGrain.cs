using Abstractions;
using Grains.States;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains;

public class RiderGrain : Grain, IRiderGrain
{
    private readonly ILogger _logger;
    private readonly RiderState _riderState;

    public RiderGrain(ILogger<RiderGrain> logger, RiderState riderState)
    {
        _logger = logger;
        _riderState = riderState;
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

    public Task<bool> IsAvailable()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsWorking()
    {
        throw new NotImplementedException();
    }

    public Task UpdateLocation(Location location)
    {
        throw new NotImplementedException();
    }
}
