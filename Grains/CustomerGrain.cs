using Abstractions;

namespace Grains;

public class CustomerGrain : Grain, ICustomerGrain
{
    public async Task<string> CreateOrder(List<string> productIds)
    {
        var orderId = $"{this.GetPrimaryKeyString()}-{Guid.NewGuid()}";
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
        await orderGrain.SetProducts(productIds);
        return orderId;
    }

    public Task<string> GetOrderStatus(string orderId)
    {
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
        return orderGrain.GetStatus();
    }
}