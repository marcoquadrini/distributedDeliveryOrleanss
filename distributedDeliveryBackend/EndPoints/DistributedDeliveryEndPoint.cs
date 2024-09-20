using System.Text.Json;
using Abstractions;
using distributedDeliveryBackend.Dto;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Orleans;


namespace distributedDeliveryBackend.EndPoints;

public static class DistributedDeliveryEndPoint
{

    public static WebApplication MapDistributedDeliveryEndpoints(this WebApplication app)
    {
        
        app.MapPost("/addOrder", (OrderEventPublisher publisher, [FromBody] AddOrderRequest data) =>
            {
                var order = new OrderDb();
                order.Name = data.name;
                order.Lastname = data.lastname;
                order.Address = data.address;
                order.City = data.city;
                order.ZipCode = data.zipCode;
                order.IdArticle = data.idArticle;
                publisher.PublishOrderCreatedEvent(order);
                return "Order added successfully";
            })
            .WithOpenApi();
        
        app.MapPost("/changeOrderStatus",  (OrderEventPublisher publisher, [FromBody] ChangeOrderStatusRequest request) =>
        {
            publisher.PublishOrderDeletedEvent(request);
            return Results.Ok($"Order id: {request.idOrder} updated to status {request.newOrderState}");
        });

        app.MapGet("/setWorkingRider", (int idRider, bool isWorking) =>
        {
            /*Gestire il set working rider*/    
        });

        app.MapGet("/getOrder", async (IGrainFactory grainFactory, string idOrder) =>
        {
            var orderGrain = grainFactory.GetGrain<IOrderGrain>(idOrder);
            var orderJson = await orderGrain.GetItem();
            return orderJson;
            try
            {
                OrderDto? order = JsonSerializer.Deserialize<OrderDto>(orderJson);
                return order?.ToString();
            }
            catch (JsonException ex)
            {
                return ($"Errore nella deserializzazione: {ex.Message}");
            }
            return "something wrong";
        });
    return app;
    }
}