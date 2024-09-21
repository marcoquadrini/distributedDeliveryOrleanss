using Abstractions;
using Grains.States;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains;

public class RiderGrain : Grain, IRiderGrain
{
    private readonly ILogger _logger;
    private readonly IPersistentState<RiderState> _riderState;

    public RiderGrain(ILogger<RiderGrain> logger, [PersistentState("Rider")] IPersistentState<RiderState> riderState)
    {
        _logger = logger;
        _riderState = riderState;
    }
    
     

    public async Task AssignOrder(string orderKey)
    {
        if (!_riderState.State.IsAvailable)
            throw new Exception("Rider is not available");

        _riderState.State.AssignedOrder = orderKey;
        _riderState.State.IsAvailable = false;
        await _riderState.WriteStateAsync();
        _logger.LogInformation($"Order {orderKey} assigned to rider {_riderState.State.Name}");
    }

    public Task<Location> GetLocation(Location location)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetName()
    {
        return Task.FromResult(_riderState.State.Name);
    }

    public Task<bool> IsAvailable()
    {
        return Task.FromResult(_riderState.State.IsAvailable);
    }

    public Task<bool> IsWorking()
    {
        return Task.FromResult(_riderState.State.IsWorking);
    }

    public async Task SetWorking(bool working)
    {
        _riderState.State.IsWorking = working;
        if (working)
            _riderState.State.IsAvailable = true;
        else
            _riderState.State.IsAvailable = false;
        await _riderState.WriteStateAsync();
    }

    public async Task SetAvailable(bool available)
    {
        _riderState.State.IsAvailable = available;
        await _riderState.WriteStateAsync();
    }

    public async Task CompleteOrder()
    {
        if (_riderState.State.AssignedOrder != "")
        {
            _riderState.State.AssignedOrder = null;
            _riderState.State.IsAvailable = true;
            await _riderState.WriteStateAsync();
        }
    }

    public async Task SetInfo(string name, string lastName, bool isWorking )
    {
        var riderState = new RiderState(name, lastName, isWorking);
        _riderState.State = riderState;
        await _riderState.WriteStateAsync();

    }

    public Task UpdateLocation(Location location)
    {
        throw new NotImplementedException();
    }
}
