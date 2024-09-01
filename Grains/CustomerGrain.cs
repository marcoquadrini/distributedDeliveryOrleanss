using Abstractions;
using Grains.States;

namespace Grains;

public class CustomerGrain : Grain<CustomerState>, ICustomerGrain
{
    public async Task<string> CreateOrder(List<string> productIds)
    {
        var orderId = $"{this.GetPrimaryKeyString()}-{Guid.NewGuid()}";
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
        await orderGrain.SetProducts(productIds);
        return orderId;
    }
    
    public async Task<string> CreateOrder(string productId)
    {
        var orderId = $"{this.GetPrimaryKeyString()}-{Guid.NewGuid()}";
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
        
        //await orderGrain.Initialize(productId);
        
        State.Orders.Add(orderId);
        await WriteStateAsync();

        return orderId;
    }

    public async Task<string> GetOrderStatus(string orderId)
    {
        if (!State.Orders.Contains(orderId))
            throw new Exception("Order not found");

        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
        return await orderGrain.GetStatus();
    }
}