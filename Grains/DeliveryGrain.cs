using Abstractions;
using distributedDeliveryBackend;
using distributedDeliveryBackend.Dto;
using Grains.Services;
using Grains.States;
using Grains.Utils;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Grains;

public class DeliveryGrain(ILogger<RiderGrain> logger, [PersistentState("Rider")] IPersistentState<DeliveryState> deliveryState, RiderAvailabilityService riderAvailabilityService, IConnectionMultiplexer redisConnection)
    : Grain, IDeliveryGrain
{
    private readonly ILogger _logger = logger;
    private readonly IDatabase _redis = redisConnection.GetDatabase();
    public async Task StartDelivery(string orderId)
    {
        deliveryState.State.OrderId = orderId;
        await deliveryState.WriteStateAsync();
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
        var deliveryStarted = await ChooseRider();
        if(deliveryStarted)
            await orderGrain.UpdateStatus(OrderStatus.InConsegna.ToString());
    }

    public async Task CompleteDelivery()
    {
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(deliveryState.State.OrderId);
        await orderGrain.UpdateStatus(OrderStatus.Consegnato.ToString());

        var riderGrain = GrainFactory.GetGrain<IRiderGrain>(deliveryState.State.RiderId);
        await riderGrain.CompleteOrder();
    }

    public async Task<bool> ChooseRider()
    {
        var availableRiders = await riderAvailabilityService.GetAvailableRiderIdsAsync();

        if (availableRiders.Count == 0)
        {
            await _redis.SetAddAsync(Constants.RedisPendingDeliveriesKey, deliveryState.State.OrderId);
            Console.WriteLine("No available riders found.");
            return false;
        }
        
        var selectedRiderId = availableRiders.First();
        Console.WriteLine($"Rider found: {selectedRiderId}");
        deliveryState.State.RiderId = selectedRiderId;
        var riderGrain = GrainFactory.GetGrain<IRiderGrain>(selectedRiderId);
        Console.WriteLine($"Rider found: {riderGrain.GetGrainId()}");
        await riderGrain.AssignOrder(deliveryState.State.OrderId);
        await deliveryState.WriteStateAsync();
        await _redis.SetRemoveAsync(Constants.RedisPendingDeliveriesKey, deliveryState.State.OrderId);
        Console.WriteLine($"Assigning Order to Rider {selectedRiderId}");
        return true;
    }

    public async Task ContinueDelivery()
    {
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(deliveryState.State.OrderId);
        var deliveryStarted = await ChooseRider();
        if(deliveryStarted)
            await orderGrain.UpdateStatus(OrderStatus.InConsegna.ToString());
    }
}