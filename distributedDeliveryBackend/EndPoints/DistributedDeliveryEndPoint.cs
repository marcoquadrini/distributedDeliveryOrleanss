using distributedDeliveryBackend.Dto;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace distributedDeliveryBackend.EndPoints;

public static class DistributedDeliveryEndPoint
{
    public static WebApplication MapDistributedDeliveryEndpoints(this WebApplication app)
    {
        app.MapGet("/provaGet", (String id) => { return "questo è l'id che mi passi" + id; })
            .WithOpenApi();

        app.MapPost("/addOrder", async (OrderEventPublisher publisher,ApplicationDbSqlContext mysql, [FromBody] AddOrderRequest data) =>
            {
                var order = new OrderDb();
                order.Name = data.name;
                order.Lastname = data.lastname;
                order.Address = data.address;
                order.City = data.city;
                order.ZipCode = data.zipCode;
                order.IdArticle = data.idArticle;
                order.Status = OrderStatus.PresoInCarico;
                mysql.orderDb.Add(order);
                await mysql.SaveChangesAsync();
                publisher.PublishOrderCreatedEvent(order);
                return "Order added successfully";
            })
            .WithOpenApi();
        
        app.MapPost("/changeOrderStatus", async (OrderEventPublisher publisher,ApplicationDbSqlContext mysql, [FromBody] ChangeOrderStatusRequest request) =>
        {
            var orderToUpdate = await mysql.orderDb.FindAsync(request.idOrder);
            if (orderToUpdate == null) 
            {
                return Results.Problem("Order not found", statusCode: 404);
            }
            orderToUpdate.Status = request.newOrderState;
            mysql.orderDb.Update(orderToUpdate);
            await mysql.SaveChangesAsync();
            if(orderToUpdate.Status == OrderStatus.Annullato) publisher.PublishOrderDeletedEvent(orderToUpdate.Id);
            return Results.Ok($"Order id: {orderToUpdate.IdArticle} updated to status {orderToUpdate.Status}");
        });

        app.MapGet("/setWorkingRider", async(ApplicationDbSqlContext mysql, int idRider, bool isWorking) =>
        {
            var rider = mysql.riderDb.Find(idRider);
            if (rider != null)
            {
                rider.IsWorking = isWorking;
                mysql.riderDb.Update(rider);
                await mysql.SaveChangesAsync();
                return "Set rider " + rider.Name + " " + rider.LastName + " Is working = " + rider.IsWorking;
            }
            else
            {
                return "Rider not found, please check the rider id";
            }
            
        });
    return app;
    }
}