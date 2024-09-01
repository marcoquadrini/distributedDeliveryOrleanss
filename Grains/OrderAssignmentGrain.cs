using Abstractions;
using Orleans;


public class OrderAssignmentGrain : Grain, IOrderAssignmentGrain
{
    
    public async Task HandleOrderCreatedEvent(int orderId)
    {
        Console.WriteLine("Sono il grain che ha lo scopo di gestire l'ordine. Id ordine " +orderId);
        //Todo gestire la creazione del grain del rider
        /*
        var riderGrain = GrainFactory.GetGrain<IRiderGrain>();
        await riderGrain.AssignOrder(order);
        */
    }

    public Task StartDelivery(string orderId, string riderId)
    {
        throw new NotImplementedException();
    }

    public Task CompleteDelivery()
    {
        throw new NotImplementedException();
    }
}