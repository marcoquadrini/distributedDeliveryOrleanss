using Abstractions;
using distributedDeliveryBackend;
using distributedDeliveryBackend.Dto;
using Grains.Services;
using Grains.States;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Grains;

public class DeliveryGrain(ILogger<RiderGrain> logger, [PersistentState("Rider")] IPersistentState<DeliveryState> deliveryState, RiderAvailabilityService riderAvailabilityService, IConnectionMultiplexer redisConnection)
    : Grain, IDeliveryGrain
{
    private readonly ILogger _logger = logger;
    private readonly RiderAvailabilityService _riderAvailabilityService = riderAvailabilityService;
    private readonly IDatabase _redis = redisConnection.GetDatabase();
    private const string RedisPendingDeliveryKey = "pending_delivery";
    public async Task StartDelivery(string orderId)
    {
        deliveryState.State.OrderId = orderId;
        await deliveryState.WriteStateAsync();
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
        await ChooseRider();
        await orderGrain.UpdateStatus(OrderStatus.InConsegna.ToString());
        
    }

    public async Task CompleteDelivery()
    {
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(deliveryState.State.OrderId);
        await orderGrain.UpdateStatus(OrderStatus.Consegnato.ToString());

        var riderGrain = GrainFactory.GetGrain<IRiderGrain>(deliveryState.State.RiderId);
        await riderGrain.CompleteOrder();
    }

    public async Task ChooseRider()
    {
        //TODO FINIRE
        var availableRiders = await _riderAvailabilityService.GetAvailableRiderIdsAsync();

        if (availableRiders.Count == 0)
        {
            await _redis.SetAddAsync(RedisPendingDeliveryKey, deliveryState.State.OrderId);
            Console.WriteLine("No available riders found.");
            return;
        }
        
        // Pick the first available rider (or use some selection algorithm)
        var selectedRiderId = availableRiders.First();
        deliveryState.State.RiderId = selectedRiderId;
        await GrainFactory.GetGrain<IRiderGrain>(selectedRiderId).AssignOrder(deliveryState.State.OrderId);
        await deliveryState.WriteStateAsync();
        await _redis.SetRemoveAsync(RedisPendingDeliveryKey, deliveryState.State.OrderId);
        Console.WriteLine($"Assigning Order to Rider {selectedRiderId}");
    }
}