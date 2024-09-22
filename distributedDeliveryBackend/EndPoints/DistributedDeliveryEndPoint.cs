using System.Data;
using System.Text.Json;
using Abstractions;
using Bogus;
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
    private const string SimulationTag = "MOCK";

    public static WebApplication MapDistributedDeliveryEndpoints(this WebApplication app)
    {
        app.MapPost("/addOrder",
                async (IGrainFactory grainFactory, OrderEventPublisher publisher, ILogger<Program> logger,
                    [FromBody] AddOrderRequest order) =>
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


        app.MapPost("/changeOrderStatus", async (IGrainFactory grainFactory, OrderEventPublisher publisher,
                ILogger<Program> logger, [FromBody] ChangeOrderStatusRequest request) =>
            {
                try
                {
                    var orderGrain = grainFactory.GetGrain<IOrderGrain>(request.IdOrder);
                    var orderUpdatedSuccess = await orderGrain.UpdateStatus(request.NewOrderState.ToString());
                    if (orderUpdatedSuccess)
                    {
                        publisher.PublishOrderDeletedEvent(request);
                        logger.LogInformation("Order {OrderId} updated to status {NewStatus}", request.IdOrder,
                            request.NewOrderState);
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
                        logger.LogInformation("Fetched order {OrderId} successfully: {orderContent}", idOrder,
                            orderContent);
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

        app.MapPost("/registerRider", async (IGrainFactory grainFactory, RiderEventPublisher publisher,
                ILogger<Program> logger, [FromBody] AddRiderRequest request) =>
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
                    logger.LogError(ex, "Error registering new rider {RiderName} {RiderLastname}", request.Name,
                        request.Lastname);
                    return Results.Problem("Error registering rider. Please try again later.", statusCode: 500);
                }
            })
            .WithTags(RiderTag)
            .WithOpenApi();

        app.MapPost("/setWorkingRider",
                (RiderEventPublisher publisher, ILogger<Program> logger, [FromBody] SetWorkingRiderRequest request) =>
                {
                    try
                    {
                        publisher.PublishSetWorkingRider(request);
                        logger.LogInformation("Sent message for setting rider {RiderId} working status to {IsWorking}",
                            request.RiderId, request.IsWorking);
                        return Results.Ok("Rider status updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error setting working status for rider {RiderId}", request.RiderId);
                        return Results.Problem("Error setting rider working status. Please try again later.",
                            statusCode: 500);
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
                    return Results.Problem("Error fetching rider working status. Please try again later.",
                        statusCode: 500);
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

        app.MapPost("/mockOrder",
                async (IGrainFactory grainFactory, OrderEventPublisher orderPublisher, ILogger<Program> logger) =>
                {
                    try
                    {
                        var faker = new Faker();

                        var customerId = faker.Internet.UserName();
                        var deliveryDetails = new AddOrderRequest.DeliveryInfo(
                            faker.Name.FirstName(),
                            faker.Name.LastName(),
                            faker.Address.StreetAddress(),
                            faker.Address.City(),
                            faker.Address.ZipCode()
                        );

                        var orderItems =
                            faker.Random.ListItems(new[] { "Pizza", "Hamburger", "Sushi", "Pasta", "Insalata", "Tacos" },
                                2);

                        var orderRequest = new AddOrderRequest
                        {
                            IdCustomer = customerId,
                            DeliveryDetails = deliveryDetails,
                            IdArticles = orderItems.ToList()
                        };

                        var customerGrain = grainFactory.GetGrain<ICustomerGrain>(customerId);
                        var orderId = await customerGrain.CreateOrder(orderRequest.IdArticles);
                        orderRequest.IdOrder = orderId;
                        orderPublisher.PublishOrderCreatedEvent(orderRequest);
                        logger.LogInformation(
                            "Mock order created successfully for customer {CustomerId} with ID: {OrderId}", customerId,
                            orderId);

                        return Results.Ok($"Mock order created successfully for customer {customerId} with ID: {orderId}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error during simulation");
                        return Results.Problem("Error during simulation. Please try again later.", statusCode: 500);
                    }
                })
            .WithTags(SimulationTag)
            .WithOpenApi();

        return app;
    }
}