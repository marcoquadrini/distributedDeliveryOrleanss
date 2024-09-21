using Abstractions;
using distributedDeliveryBackend;
using distributedDeliveryBackend.Dto;
using Grains.Services;
using Grains.States;
using Grains.Utils;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Grains;

public class DeliveryGrain(ILogger<RiderGrain> logger, [PersistentState("Delivery")] IPersistentState<DeliveryState> deliveryState, RiderAvailabilityService riderAvailabilityService, IConnectionMultiplexer redisConnection)
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
        if(deliveryStarted != null)
            await orderGrain.UpdateStatus(OrderStatus.InConsegna.ToString());
    }

    public async Task CompleteDelivery()
    {
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(deliveryState.State.OrderId);
        await orderGrain.UpdateStatus(OrderStatus.Consegnato.ToString());

        var riderGrain = GrainFactory.GetGrain<IRiderGrain>(deliveryState.State.RiderId);
        await riderGrain.CompleteOrder();
    }

    public async Task<string?> ChooseRider()
    {
        var availableRiders = await riderAvailabilityService.GetAvailableRiderIdsAsync();

        if (availableRiders.Count == 0)
        {
            await _redis.SetAddAsync(Constants.RedisPendingDeliveriesKey, deliveryState.State.OrderId);
            Console.WriteLine("No available riders found.");
            return null;
        }
        
        var selectedRiderId = availableRiders.First();
        Console.WriteLine($"Rider found: {selectedRiderId}");
        deliveryState.State.RiderId = selectedRiderId;
        
        
        var riderGrain = GrainFactory.GetGrain<IRiderGrain>(selectedRiderId);
        Console.WriteLine($"Rider found: {riderGrain.GetGrainId()}");
        Console.WriteLine($"Pending to delete : {deliveryState.State.OrderId}");
        
        
        
        await _redis.SetRemoveAsync(Constants.RedisPendingDeliveriesKey, deliveryState.State.OrderId);
        await deliveryState.WriteStateAsync();
       
        Console.WriteLine($"Assigning Order to Rider {selectedRiderId}");
        return deliveryState.State.OrderId;
    }

    public async Task<string?> ContinueDelivery()
    {
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(deliveryState.State.OrderId);
        var deliveryStarted = await ChooseRider();
        if (deliveryStarted != null)
        {
            await orderGrain.UpdateStatus(OrderStatus.InConsegna.ToString());
            return deliveryStarted;
        }
        return null;
    }

    public Task<string> GetOrderId()
    {
        return Task.FromResult(deliveryState.State.OrderId);
    }
}