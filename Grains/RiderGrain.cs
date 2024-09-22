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
        _logger.LogInformation("Attempting to assign order {OrderKey} to rider {RiderId}", orderKey, this.GetPrimaryKeyString());

            if (!_riderState.State.IsAvailable)
            {
                _logger.LogWarning("Rider {RiderId} is unavailable to take the order {OrderKey}", this.GetPrimaryKeyString(), orderKey);
                throw new Exception("Rider is not available");
            }

            try
            {
                _logger.LogInformation("Assigning order {OrderKey} to rider {RiderId}", orderKey, this.GetPrimaryKeyString());
                _riderState.State.AssignedOrder = orderKey;
                await SetAvailable(false);
                await _riderState.WriteStateAsync();
                _logger.LogInformation("Order {OrderKey} successfully assigned to rider {RiderName} (ID: {RiderId})", orderKey, _riderState.State.Name, this.GetPrimaryKeyString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign order {OrderKey} to rider {RiderId}", orderKey, this.GetPrimaryKeyString());
                throw;
            }
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
        _logger.LogInformation("Setting working status to {WorkingStatus} for rider {RiderId}", working, this.GetPrimaryKeyString());

        try
        {
            _riderState.State.IsWorking = working;
            await _riderState.WriteStateAsync();

            await SetAvailable(working);
            _logger.LogInformation("Successfully updated working status for rider {RiderId}", this.GetPrimaryKeyString());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update working status for rider {RiderId}", this.GetPrimaryKeyString());
            return false;
        }
    }

    public async Task SetAvailable(bool available)
    {
        var riderId = this.GetPrimaryKeyString();
        _logger.LogInformation("Setting availability to {Availability} for rider {RiderId}", available, riderId);

        try
        {
            if (available)
            {
                await _redis.SetAddAsync(Constants.RedisAvailableRidersKey, riderId);
            }
            else
            {
                await _redis.SetRemoveAsync(Constants.RedisAvailableRidersKey, riderId);
            }

            _riderState.State.IsAvailable = available;
            await _riderState.WriteStateAsync();

            _logger.LogInformation("Rider {RiderId} availability updated to {Availability}", riderId, available);

            if (available)
            {
                await CheckPendingDeliveries();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting availability for rider {RiderId}", riderId);
            throw;
        }
    }

    public async Task CompleteOrder()
    {
        if (!string.IsNullOrEmpty(_riderState.State.AssignedOrder))
        {
            try
            {
                _riderState.State.AssignedOrder = null;

                if (_riderState.State.IsWorking)
                {
                    await SetAvailable(true);
                }
                await _riderState.WriteStateAsync();
                _logger.LogInformation("Rider {RiderId} has completed the order", this.GetPrimaryKeyString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to complete the order for rider {RiderId}", this.GetPrimaryKeyString());
                throw;
            }
        }
        else
        {
            _logger.LogWarning("No assigned order found for rider {RiderId} to complete", this.GetPrimaryKeyString());
        }
    }

    public async Task CheckPendingDeliveries()
    {
        _logger.LogInformation("Checking for pending deliveries");
        try
        {
            var pendingDeliveries = await _pendingDeliveriesService.GetPendingDeliveriesAsync();

            if (pendingDeliveries.Count > 0)
            {
                _logger.LogInformation("Found pending deliveries");

                var delivery = GrainFactory.GetGrain<IDeliveryGrain>(pendingDeliveries.First());
                await delivery.ContinueDelivery();
            }
            else
            {
                _logger.LogInformation("No pending deliveries found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking pending deliveries for rider {RiderId}", this.GetPrimaryKeyString());
            throw;
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
