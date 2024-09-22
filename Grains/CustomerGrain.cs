using Abstractions;
using Grains.States;

namespace Grains;

public class CustomerGrain : Grain<CustomerState>, ICustomerGrain
{
    public async Task<string> CreateOrder(List<string> products)
    {
        var orderId = $"{this.GetPrimaryKeyString()}-{Guid.NewGuid()}";
        var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderId);
        await orderGrain.SetProducts(products);
        return orderId;
    }
    
}