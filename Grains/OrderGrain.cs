using Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using distributedDeliveryBackend.Dto;
using distributedDeliveryBackend.Dto.Enums;
using Grains.States;

namespace Grains
{
    public class OrderGrain : Orleans.Grain, IOrderGrain
    {
        private readonly ILogger _logger;
        private readonly  IPersistentState<OrderState> _orderState;

        public OrderGrain(ILogger<OrderGrain> logger, [PersistentState("Order")] IPersistentState<OrderState> state)
        {
            _logger = logger;
            _orderState = state;
        }

        public async Task AddItem(string item)
        {
            try
            {
                _logger.LogInformation("Adding item {Item} to order {OrderId}", item, this.GetPrimaryKeyString());
                _orderState.State.ProductIds.Add(item);
                await _orderState.WriteStateAsync();
                _logger.LogInformation("Item {Item} added successfully to order {OrderId}", item, this.GetPrimaryKeyString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item {Item} to order {OrderId}", item, this.GetPrimaryKeyString());
                throw;
            }
        }

        public Task<string> GetItemList()
        {
            var itemList = string.Join(", ", _orderState.State.ProductIds);
            return Task.FromResult(itemList);
        }

        public Task<string> GetStatus()
        {
            return Task.FromResult(_orderState.State.Status);
        }

        public async Task AssignToRider(string riderId)
        {
            _logger.LogInformation("Assigning order {OrderId} to rider {RiderId}", this.GetPrimaryKeyString(), riderId);
            try
            {
                var riderGrain = GrainFactory.GetGrain<IRiderGrain>(riderId);
                _logger.LogInformation("Fetched rider grain {RiderId} for order {OrderId}", riderId, this.GetPrimaryKeyString());

                _orderState.State.RiderId = riderId;
                _orderState.State.Status = OrderStatus.InConsegna.ToString();
                await _orderState.WriteStateAsync();

                riderGrain.AssignOrder(this.GetPrimaryKeyString());
                _logger.LogInformation("Order {OrderId} assigned to rider {RiderId}, status set to {Status}", this.GetPrimaryKeyString(), riderId, _orderState.State.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign order {OrderId} to rider {RiderId}", this.GetPrimaryKeyString(), riderId);
                throw;
            }
        }

        public async Task SetProducts(List<string> productIds)
        {
            try
            {
                var productList = string.Join(", ", productIds);
                _logger.LogInformation("Setting product list for order {OrderId}: {ProductList}", this.GetPrimaryKeyString(), productList);
                _orderState.State.ProductIds = productIds;
                await _orderState.WriteStateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set products for order {OrderId}", this.GetPrimaryKeyString());
                throw;
            }
        }

        public async Task<bool> UpdateStatus(string status)
        {
            try
            {
                _orderState.State.Status = status;
                await _orderState.WriteStateAsync();
                _logger.LogInformation("Status updated to {Status} for order {OrderId}", status, this.GetPrimaryKeyString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for order {OrderId}", this.GetPrimaryKeyString());
                return false;
            }
        }
    }
}
