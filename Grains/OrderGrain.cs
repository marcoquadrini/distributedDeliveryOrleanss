using Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using distributedDeliveryBackend.Dto;
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

        //TODO MODIFICARE
        public async Task AssignToRider(string riderId)
        {
            //if (_orderState.State.Status != OrderStatus.Confermato.ToString())
            //    throw new Exception("Order is not in a pending state");

            Console.WriteLine($"Assegnando l'ordine al rider: {riderId}");
            
            var riderGrain = GrainFactory.GetGrain<IRiderGrain>(riderId);
            
            Console.WriteLine($"1Grain preso: {riderGrain.GetGrainId()}");
            /*var isAvailable = await riderGrain.IsAvailable();

            if (!isAvailable)
            {
                Console.WriteLine("Rider is not available");
                throw new Exception("Rider is not available");
            }*/
            Console.WriteLine($"2Grain preso: {riderGrain.GetGrainId()}");
            _orderState.State.RiderId = riderId;
            _orderState.State.Status = OrderStatus.InConsegna.ToString();
            Console.WriteLine($"Rider dell'ordine impostato: {riderId}, stato {_orderState.State.Status}");
            await _orderState.WriteStateAsync();
            Console.WriteLine($"Per assegnare l'ordine al rider: {riderGrain.GetGrainId()}");
            riderGrain.AssignOrder(this.GetPrimaryKeyString());
            //var deliveryGrain = GrainFactory.GetGrain<IDeliveryGrain>(this.GetPrimaryKeyString());
            //await deliveryGrain.StartDelivery(this.GetPrimaryKeyString());
        }

        public async Task SetProducts(List<string> productIds)
        {
            _orderState.State.ProductIds = productIds;
            await _orderState.WriteStateAsync();
        }

        public async Task<bool> UpdateStatus(string status)
        {
            try
            {
                _orderState.State.Status = status;
                await _orderState.WriteStateAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task SetLocation(Location location)
        {
            throw new NotImplementedException();
        }
    }
}
