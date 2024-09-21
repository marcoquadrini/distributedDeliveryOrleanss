using System.Data;
using System.Text.Json;
using Abstractions;
using distributedDeliveryBackend.Dto;
using distributedDeliveryBackend.Dto.Request;
using distributedDeliveryBackend.Publishers;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Orleans;


namespace distributedDeliveryBackend.EndPoints;

public static class DistributedDeliveryEndPoint
{
    private const string riderTag = "RIDER"; 
    private const string ordertag = "ORDER"; 
    public static WebApplication MapDistributedDeliveryEndpoints(this WebApplication app)
    {
        
        app.MapPost("/addOrder", async (IGrainFactory grainFactory,OrderEventPublisher publisher, [FromBody] AddOrderRequest order) =>
            {
                var customer = grainFactory.GetGrain<ICustomerGrain>(order.IdCustomer);
                var orderId = await customer.CreateOrder(order.IdArticles);
                order.IdOrder = orderId;
                publisher.PublishOrderCreatedEvent(order);
                return order.DeliveryDetails.Name;
            })
            .WithTags(ordertag)
            .WithOpenApi();
        
     
        app.MapPost("/changeOrderStatus", async (IGrainFactory grainFactory, OrderEventPublisher publisher, [FromBody] ChangeOrderStatusRequest request) =>
        {
            var orderGrain = grainFactory.GetGrain<IOrderGrain>(request.IdOrder);
            var orderUpdatedSuccess = await orderGrain.UpdateStatus(request.NewOrderState.ToString());
            if (orderUpdatedSuccess)
            {
                publisher.PublishOrderDeletedEvent(request);
                return Results.Ok($"Order id: {request.IdOrder} updated to status {request.NewOrderState}");
            }
            return Results.Problem("Something went wrong updating the order status.");
        })
        .WithTags(ordertag)
        .WithOpenApi();



        app.MapGet("/getOrder", async (IGrainFactory grainFactory, string idOrder) =>
            {
                var orderGrain = grainFactory.GetGrain<IOrderGrain>(idOrder);
                var orderJson = await orderGrain.GetItem();
                return orderJson;
            })
            .WithTags(ordertag)
            .WithOpenApi();
        
    
        app.MapPost("/registerRider", async (IGrainFactory grainFactory, RiderEventPublisher publisher, [FromBody] AddRiderRequest request) =>
            {
                var newRiderId = Guid.NewGuid().ToString();
                var riderGrain = grainFactory.GetGrain<IRiderGrain>(newRiderId);
                var setRiderSuccess = await riderGrain.SetInfo(request.Name, request.Lastname, request.IsWorking);
                if (setRiderSuccess)
                {
                    if (request.IsWorking) {
                        var setWorkingRiderRequest = new SetWorkingRiderRequest(newRiderId, true);
                        publisher.PublishSetWorkingRider(setWorkingRiderRequest);
                    }
                    return "Set new rider success";
                }
                return "Something wrong";
                
            })
            .WithTags(riderTag)
            .WithOpenApi();
        
        
        app.MapPost("/setWorkingRider", async (IGrainFactory grainFactory, RiderEventPublisher publisher,
                [FromBody] SetWorkingRiderRequest request) =>
            {
                var riderGrain = grainFactory.GetGrain<IRiderGrain>(request.RiderId);
                var setWorkingRiderSuccess = await riderGrain.SetWorking(request.IsWorking);
                if (setWorkingRiderSuccess)
                {
                    //publisher.PublishSetWorkingRider(request);
                    return "Ok";
                }

                return "Something wrong";
            })
            .WithTags(riderTag)
            .WithOpenApi();
        
        app.MapGet("/getIsWorking", async (IGrainFactory grainFactory, string idRider) =>
            {
                var riderGrain = grainFactory.GetGrain<IRiderGrain>(idRider);
                var orderJson = await riderGrain.IsWorking();
                return orderJson;
            })
            .WithTags(riderTag)
            .WithOpenApi();

        app.MapPost("/deliverOrder", async (IGrainFactory grainFactory, string idOrder) =>
            {
                var deliveryGrain = grainFactory.GetGrain<IDeliveryGrain>(idOrder);
                await deliveryGrain.CompleteDelivery();
                return "Order delivered";
            }).WithTags(ordertag)
            .WithOpenApi();
        
        
        return app;
        
    }
}