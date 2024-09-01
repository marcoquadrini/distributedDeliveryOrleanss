using Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grains.States;

namespace Grains
{
    public class OrderGrain : Orleans.Grain, IOrderGrain
    {
        private readonly ILogger logger;
        private readonly  IPersistentState<OrderState> _orderState;

        public OrderGrain(ILogger<OrderGrain> logger, [PersistentState("Order")] IPersistentState<OrderState> state)
        {
            this.logger = logger;
            _orderState = state;
        }

        public async Task AddItem(string item)
        {
            _orderState.State.ProductIds.Add(item);
            await _orderState.WriteStateAsync();
        }

        public Task<string> GetItemList()
        {
            var itemList = string.Join(", ", _orderState.State.ProductIds);
            return Task.FromResult(itemList);
        }

        public Task<Location> GetLocation()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetStatus()
        {
            return Task.FromResult(_orderState.State.Status);
        }

        public Task AssignToRider(string riderId)
        {
               throw new NotImplementedException();
        }

        public Task SetProducts(List<string> productIds)
        {
            throw new NotImplementedException();
        }

        public Task SetLocation(Location location)
        {
            throw new NotImplementedException();
        }
    }
}
