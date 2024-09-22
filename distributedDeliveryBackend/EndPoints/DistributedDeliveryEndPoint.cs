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
    private const string RiderTag = "RIDER"; 
    private const string OrderTag = "ORDER"; 
    
    public static WebApplication MapDistributedDeliveryEndpoints(this WebApplication app)
    {
        app.MapPost("/addOrder", async (IGrainFactory grainFactory, OrderEventPublisher publisher, ILogger<Program> logger, [FromBody] AddOrderRequest order) =>
            {
                try
                {
                    var customer = grainFactory.GetGrain<ICustomerGrain>(order.IdCustomer);
                    var orderId = await customer.CreateOrder(order.IdArticles);
                    order.IdOrder = orderId;
                    publisher.PublishOrderCreatedEvent(order);
                    logger.LogInformation("Order created successfully for customer {CustomerId}", order.IdCustomer);
                    return Results.Ok($"Order created successfully for customer {order.IdCustomer}. ID: {orderId}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error adding order for customer {CustomerId}", order.IdCustomer);
                    return Results.Problem("Error adding order. Please try again later.", statusCode: 500);
                }
            })
            .WithTags(OrderTag)
            .WithOpenApi();
        
     
        app.MapPost("/changeOrderStatus", async (IGrainFactory grainFactory, OrderEventPublisher publisher, ILogger<Program> logger, [FromBody] ChangeOrderStatusRequest request) =>
            {
                try
                {
                    var orderGrain = grainFactory.GetGrain<IOrderGrain>(request.IdOrder);
                    var orderUpdatedSuccess = await orderGrain.UpdateStatus(request.NewOrderState.ToString());
                    if (orderUpdatedSuccess)
                    {
                        publisher.PublishOrderDeletedEvent(request);
                        logger.LogInformation("Order {OrderId} updated to status {NewStatus}", request.IdOrder, request.NewOrderState);
                        return Results.Ok($"Order id: {request.IdOrder} updated to status {request.NewOrderState}");
                    }
                    return Results.BadRequest($"Failed to update order id: {request.IdOrder}.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error updating order status for order {OrderId}", request.IdOrder);
                    return Results.Problem("Error updating order status. Please try again later.", statusCode: 500);
                }
            })
        .WithTags(OrderTag)
        .WithOpenApi();
        
        app.MapGet("/getOrderContents", async (IGrainFactory grainFactory, ILogger<Program> logger, string idOrder) =>
            {
                try
                {
                    var orderGrain = grainFactory.GetGrain<IOrderGrain>(idOrder);
                    var orderContent = await orderGrain.GetItemList();
                    if (orderContent is { Length: > 0 })
                    {
                        logger.LogInformation("Fetched order {OrderId} successfully: {orderContent}", idOrder, orderContent);
                        return Results.Ok(orderContent);
                    }
                    return Results.NotFound($"Order with id {idOrder} not found.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error fetching order {OrderId}", idOrder);
                    return Results.Problem("Error fetching order. Please try again later.", statusCode: 500);
                }
            })
            .WithTags(OrderTag)
            .WithOpenApi();
        
        app.MapPost("/registerRider", async (IGrainFactory grainFactory, RiderEventPublisher publisher, ILogger<Program> logger, [FromBody] AddRiderRequest request) =>
            {
                try
                {
                    var newRiderId = Guid.NewGuid().ToString();
                    var riderGrain = grainFactory.GetGrain<IRiderGrain>(newRiderId);
                    var setRiderSuccess = await riderGrain.SetInfo(request.Name, request.Lastname, request.IsWorking);
                    if (setRiderSuccess)
                    {
                        if (request.IsWorking)
                        {
                            var setWorkingRiderRequest = new SetWorkingRiderRequest(newRiderId, true);
                            publisher.PublishSetWorkingRider(setWorkingRiderRequest);
                        }
                        logger.LogInformation("New rider {RiderId} registered successfully", newRiderId);
                        return Results.Ok("New rider registered");
                    }
                    return Results.BadRequest("Failed to set rider information.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error registering new rider {RiderName} {RiderLastname}", request.Name, request.Lastname);
                    return Results.Problem("Error registering rider. Please try again later.", statusCode: 500);
                }
            })
            .WithTags(RiderTag)
            .WithOpenApi();
        
        app.MapPost("/setWorkingRider", (RiderEventPublisher publisher, ILogger<Program> logger, [FromBody] SetWorkingRiderRequest request) =>
            {
                try
                {
                    publisher.PublishSetWorkingRider(request);
                    logger.LogInformation("Sent message for setting rider {RiderId} working status to {IsWorking}", request.RiderId, request.IsWorking);
                    return Results.Ok("Rider status updated successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error setting working status for rider {RiderId}", request.RiderId);
                    return Results.Problem("Error setting rider working status. Please try again later.", statusCode: 500);
                }
            })
            .WithTags(RiderTag)
            .WithOpenApi();
        
        app.MapGet("/getIsWorking", async (IGrainFactory grainFactory, ILogger<Program> logger, string idRider) =>
            {
                try
                {
                    var riderGrain = grainFactory.GetGrain<IRiderGrain>(idRider);
                    var isWorking = await riderGrain.IsWorking();
                    logger.LogInformation("Working status for rider {RiderId}: {isWorking}", idRider, isWorking);
                    return Results.Ok(isWorking);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error fetching working status for rider {RiderId}", idRider);
                    return Results.Problem("Error fetching rider working status. Please try again later.", statusCode: 500);
                }
            })
            .WithTags(RiderTag)
            .WithOpenApi();


        app.MapPost("/deliverOrder", async (IGrainFactory grainFactory, string idOrder) =>
            {
                var deliveryGrain = grainFactory.GetGrain<IDeliveryGrain>(idOrder);
                await deliveryGrain.CompleteDelivery();
                return "Order delivered"; 
            }).WithTags(OrderTag)
            .WithOpenApi();
        
        
        return app;
        
    }
}