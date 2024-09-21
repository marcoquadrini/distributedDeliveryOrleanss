using Abstractions;
using distributedDeliveryBackend;
using distributedDeliveryBackend.Dto;
using distributedDeliveryBackend.EndPoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Distributed Delivery",
        Description = "A simple example ASP.NET Core Web API using Grains e Silos",
        
        Contact = new OpenApiContact
        {
            Name = "Marco Quadrini - Matteo Scoccia",
            Email = "marco.quadrini@studenti.unicam.it"
        },
 
    });
});

/*
builder.Services.AddDbContext<ApplicationDbSqlContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MySql"), 
        new MySqlServerVersion(new Version(8, 0, 21))));
        */

// Configura Orleans Client
builder.Host.UseOrleansClient(clientBuilder =>
{
    clientBuilder.UseLocalhostClustering();
});




//Aggiunge il Publisher e il Subscriber di RabbitMq
builder.Services.AddSingleton<OrderEventPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapDistributedDeliveryEndpoints();

await app.RunAsync();
