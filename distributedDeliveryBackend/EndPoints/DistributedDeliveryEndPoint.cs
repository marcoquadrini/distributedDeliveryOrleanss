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

        app.MapPost("/addOrder", async (ApplicationDbSqlContext mysql, [FromBody] AddOrderRequest data) =>
            {
                var order = new OrderDb();
                order.Name = data.name;
                order.Lastname = data.lastname;
                order.Address = data.address;
                order.City = data.city;
                order.ZipCode = data.zipCode;
                order.IdArticle = data.idArticle;

                mysql.orderDb.Add(order);
                await mysql.SaveChangesAsync();
                return "Order added successfully";
            })
            .WithOpenApi();
        app.MapPost("/addNewRider", async (ApplicationDbSqlContext mysql, [FromBody] AddRiderRequest data) =>
        {
            var newRider = new RiderDb(data.name, data.lastname, data.isWorking);
            mysql.riderDb.Add(newRider);
            await mysql.SaveChangesAsync();
        });

        app.MapGet("/setWorkingRider", async(ApplicationDbSqlContext mysql, int idRider, bool isWorking) =>
        {
            var rider = mysql.riderDb.Find(idRider);
            if (rider != null)
            {
                rider.IsWorking = isWorking;
                mysql.riderDb.Update(rider);
                await mysql.SaveChangesAsync();
                return "Set rider" + rider.Name + " " + rider.LastName + " Is working = " + rider.IsWorking;
            }
            else
            {
                return "Rider not found, please check the rider id";
            }
            
        });
    return app;
    }
}