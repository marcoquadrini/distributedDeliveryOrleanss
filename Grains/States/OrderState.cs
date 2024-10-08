﻿using distributedDeliveryBackend.Dto;
using distributedDeliveryBackend.Dto.Enums;

namespace Grains.States;

[GenerateSerializer]
public class OrderState
{
    [Id(0)]
    public List<string> Products { get; set; } = new();
    
    [Id(1)]
    public string Status { get; set; } = OrderStatus.InSospeso.ToString();
    
    [Id(2)]
    public string RiderId { get; set; }
    
    [Id(3)]
    public int CustomerId { get; set; }
    
    [Id(4)]
    public DateTime CreatedAt { get; set; }
    
    [Id(5)]
    public string Address { get; set; }
    
    [Id(6)]
    public string City { get; set; }
    
    [Id(7)]
    public string ZipCode { get; set; }

    public override string ToString()
    {
        string productList = Products != null && Products.Any() 
            ? string.Join(", ", Products) 
            : "No products";

        return $"OrderState: \n" +
               $"Status: {Status}\n" +
               $"RiderId: {RiderId ?? "Not assigned"}\n" +
               $"CustomerId: {CustomerId}\n" +
               $"CreatedAt: {CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
               $"Address: {Address}, {City}, {ZipCode}\n" +
               $"Products: {productList}";
    }
}