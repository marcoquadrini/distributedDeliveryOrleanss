using Abstractions;
using distributedDeliveryBackend;
using distributedDeliveryBackend.Dto;
using distributedDeliveryBackend.EndPoints;
using distributedDeliveryBackend.Publishers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Distributed Delivery",
        Description = "A Web Api for managing grains",
        
        Contact = new OpenApiContact
        {
            Name = "Marco Quadrini - Matteo Scoccia",
            Email = "marco.quadrini@studenti.unicam.it"
        },
 
    });
    options.EnableAnnotations();
});


// Configuring Orleans Client
builder.Host.UseOrleansClient(clientBuilder =>
{
    clientBuilder.UseLocalhostClustering();
});


// Adding RabbitMq Publishers
builder.Services.AddSingleton<OrderEventPublisher>();
builder.Services.AddSingleton<RiderEventPublisher>();

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
