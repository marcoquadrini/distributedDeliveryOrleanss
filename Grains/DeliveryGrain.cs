using Abstractions;
using distributedDeliveryBackend.Dto.Enums;
using Grains.Services;
using Grains.States;
using Microsoft.Extensions.Logging;

namespace Grains;

public class DeliveryGrain(ILogger<RiderGrain> logger, [PersistentState("Delivery")] IPersistentState<DeliveryState> deliveryState, RiderAvailabilityService riderAvailabilityService, PendingDeliveriesService pendingDeliveriesService)
    : Grain, IDeliveryGrain
{
    private readonly ILogger _logger = logger;
    private readonly PendingDeliveriesService _pendingDeliveriesService = pendingDeliveriesService;

    public async Task StartDelivery(string orderId)
    {
        _logger.LogInformation("Starting delivery process for order {OrderId}", orderId);

        try
        {
            deliveryState.State.OrderId = orderId;
            await deliveryState.WriteStateAsync();

            var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
            var selectedRiderId = await ChooseRider();

            if (selectedRiderId != null)
            {
                await orderGrain.AssignToRider(selectedRiderId);
                _logger.LogInformation("Order {OrderId} successfully assigned to rider {RiderId}", orderId, selectedRiderId);
            }
            else
            {
                _logger.LogWarning("No available riders for order {OrderId}", orderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during starting delivery for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task CompleteDelivery()
    {
        _logger.LogInformation("Completing delivery for order {OrderId}", deliveryState.State.OrderId);

        try
        {
            var orderGrain = GrainFactory.GetGrain<IOrderGrain>(deliveryState.State.OrderId);
            await orderGrain.UpdateStatus(OrderStatus.Consegnato.ToString());

            var riderGrain = GrainFactory.GetGrain<IRiderGrain>(deliveryState.State.RiderId);
            await riderGrain.CompleteOrder();

            _logger.LogInformation("Order {OrderId} marked as delivered", deliveryState.State.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while completing delivery for order {OrderId}", deliveryState.State.OrderId);
            throw;
        }
    }

    public async Task<string?> ChooseRider()
    {
        _logger.LogInformation("Choosing rider for order {OrderId}", deliveryState.State.OrderId);

        try
        {
            var availableRiders = await riderAvailabilityService.GetAvailableRiderIdsAsync();

            if (availableRiders.Count == 0)
            {
                _logger.LogWarning("No available riders for order {OrderId}. Adding to pending deliveries.", deliveryState.State.OrderId);
                await _pendingDeliveriesService.AddPendingDeliveryAsync(deliveryState.State.OrderId);
                return null;
            }

            var selectedRiderId = availableRiders.First();
            _logger.LogInformation("Rider {RiderId} selected for order {OrderId}", selectedRiderId, deliveryState.State.OrderId);

            deliveryState.State.RiderId = selectedRiderId;
            _logger.LogInformation("Assigning order {OrderId} to rider {RiderId}", deliveryState.State.OrderId, selectedRiderId);

            await _pendingDeliveriesService.RemovePendingDeliveryAsync(deliveryState.State.OrderId);
            await deliveryState.WriteStateAsync();

            _logger.LogInformation("Order {OrderId} assigned to rider {RiderId} and removed from pending deliveries.", deliveryState.State.OrderId, selectedRiderId);

            return selectedRiderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while choosing rider for order {OrderId}", deliveryState.State.OrderId);
            throw;
        }
    }

    public async Task<string?> ContinueDelivery()
    {
        _logger.LogInformation("Continuing delivery for order {OrderId}", deliveryState.State.OrderId);

        try
        {
            var orderGrain = GrainFactory.GetGrain<IOrderGrain>(deliveryState.State.OrderId);
            var selectedRiderId = await ChooseRider();

            if (selectedRiderId != null)
            {
                await orderGrain.AssignToRider(selectedRiderId);
                _logger.LogInformation("Order {OrderId} successfully reassigned to rider {RiderId}", deliveryState.State.OrderId, selectedRiderId);
                return selectedRiderId;
            }
            
            _logger.LogWarning("No available riders for order {OrderId} during continuation", deliveryState.State.OrderId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during continuation of delivery for order {OrderId}", deliveryState.State.OrderId);
            throw;
        }
    }

    public Task<string> GetOrderId()
    {
        return Task.FromResult(deliveryState.State.OrderId);
    }
}