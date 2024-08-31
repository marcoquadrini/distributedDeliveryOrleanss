using distributedDeliveryBackend.Dto;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace distributedDeliveryBackend.EndPoints;

public static class DistributedDeliveryEndPoint
{
    public static WebApplication MapDistributedDeliveryEndpoints(this WebApplication app)
    {
        app.MapGet("/provaGet", (String id) =>
            {
                return "questo è l'id che mi passi" + id;

            })
            .WithOpenApi();

        app.MapPost("/provaPost", async (IConnectionMultiplexer redis,[FromBody] RequestData data) =>
            {
                var db = redis.GetDatabase();
                string val = await db.StringGetAsync("provaGet");
                return $"Questo è l'ID: {data.Id},questo è il nome: {data.Name}, e questo è quello che sta nel db redis {val}"; 
            })
            .WithOpenApi();
        
        return app;
    }
}