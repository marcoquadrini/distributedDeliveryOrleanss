using Abstractions;
using Grains.Services;
using Grains.States;
using Grains.Utils;
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

    private readonly PendingDeliveriesService _pendingDeliveriesService;

    public RiderGrain(ILogger<RiderGrain> logger, [PersistentState("Rider")] IPersistentState<RiderState> riderState, IConnectionMultiplexer redisConnection, PendingDeliveriesService pendingDeliveriesService)
    {
        _logger = logger;
        _riderState = riderState;
        _redis = redisConnection.GetDatabase();
        _pendingDeliveriesService = pendingDeliveriesService;
    }
    

    public async Task AssignOrder(string orderKey)
    {
        Console.WriteLine("Entrato in AssignOrder di RiderGrain");
        if (!_riderState.State.IsAvailable)
        {
            Console.WriteLine("Rider unavailable");
            throw new Exception("Rider is not available");
        }
        Console.WriteLine("Assigning order");
        _riderState.State.AssignedOrder = orderKey;
        Console.WriteLine("Order assigned, setting available to false");
        await SetAvailable(false);
        Console.WriteLine("Set available to false");
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
        Console.WriteLine("Entro set Working");
        try
        {
            Console.WriteLine("Entro set Working TRY ");
            _riderState.State.IsWorking = working;
            await _riderState.WriteStateAsync();
            if (working)
                await SetAvailable(true);
            else
                await SetAvailable(false);
            Console.WriteLine("Entro set Working FINITO TRY ");
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task SetAvailable(bool available)
    {
        Console.WriteLine("sono dentro set available");
        var riderId = this.GetPrimaryKeyString();
        if (available)
        {
            await _redis.SetAddAsync(Constants.RedisAvailableRidersKey, riderId);
        }
        else
        {
            Console.WriteLine("Provo ad eliminare");
            await _redis.SetRemoveAsync(Constants.RedisAvailableRidersKey, riderId);
        }
        _riderState.State.IsAvailable = available;
        await _riderState.WriteStateAsync();
        if (_riderState.State.IsAvailable)
        {
            //Todo provare a spostarlo fuori da questa chiamata
            await CheckPendingDeliveries();
        }
    }

    public async Task CompleteOrder()
    {
        if (_riderState.State.AssignedOrder != "")
        {
            _riderState.State.AssignedOrder = null;
            if (_riderState.State.IsWorking)
            {
                await SetAvailable(true);
                _riderState.State.IsAvailable = true;
            }
            await _riderState.WriteStateAsync();
        }
    }

    public async Task CheckPendingDeliveries()
    {
        Console.WriteLine("Cercando consegne in sospeso");
        var pendingDeliveries = await _pendingDeliveriesService.GetPendingDeliveriesAsync();

        if (pendingDeliveries.Count > 0)
        {
            Console.WriteLine("C'è una consegna in sospeso");
            Console.WriteLine(pendingDeliveries.First());
            var delivery = GrainFactory.GetGrain<IDeliveryGrain>(pendingDeliveries.First());
            await delivery.ContinueDelivery();
            //var orderToAssing = await delivery.ContinueDelivery();
            //if (orderToAssing != null) AssignOrder(orderToAssing);
        }
    }

    public async Task<bool> SetInfo(string name, string lastName, bool isWorking)
    {
        try
        {
            var riderState = new RiderState(name, lastName, isWorking);
            _riderState.State = riderState;
            _riderState.State.IsAvailable = false;
            await _riderState.WriteStateAsync();
            return true; 
        }
        catch (Exception)
        {
            return false; 
        }
    }

    
}
