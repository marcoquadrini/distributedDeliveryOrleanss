using Abstractions;
using Grains.States;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Orleans;
using StackExchange.Redis;

namespace Grains;

public class RiderGrain : Grain, IRiderGrain
{
    private readonly ILogger _logger;
    private readonly IPersistentState<RiderState> _riderState;
    
    private readonly IDatabase _redis;
    private const string RedisAvailableRidersKey = "available_riders";

    public RiderGrain(ILogger<RiderGrain> logger, [PersistentState("Rider")] IPersistentState<RiderState> riderState, IConnectionMultiplexer redisConnection)
    {
        _logger = logger;
        _riderState = riderState;
        _redis = redisConnection.GetDatabase();
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
    
    public Task<string> GetName()
    {
        return Task.FromResult(_riderState.State.Name + " " + _riderState.State.LastName);
    }

    public Task<bool> IsAvailable()
    {
        return Task.FromResult(_riderState.State.IsAvailable);
    }

    public Task<bool> IsWorking()
    {
        return Task.FromResult(_riderState.State.IsWorking);
    }

    public async Task<bool> SetWorking(bool working)
    {
        try
        {
            _riderState.State.IsWorking = working;
            if (working)
                await SetAvailable(true);
            else
                await SetAvailable(false);
            await _riderState.WriteStateAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task SetAvailable(bool available)
    {
        var riderId = this.GetPrimaryKeyString();
        if (available)
        {
            await _redis.SetAddAsync(RedisAvailableRidersKey, riderId);
        }
        else
        {
            await _redis.SetRemoveAsync(RedisAvailableRidersKey, riderId);
        }
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

    public async Task<bool> SetInfo(string name, string lastName, bool isWorking)
    {
        try
        {
            var riderState = new RiderState(name, lastName, isWorking);
            _riderState.State = riderState;
            await _riderState.WriteStateAsync();
            return true; 
        }
        catch (Exception)
        {
            return false; 
        }
    }

    
}
