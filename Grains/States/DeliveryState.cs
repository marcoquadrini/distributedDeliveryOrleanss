namespace Grains.States;

[GenerateSerializer]
public class DeliveryState
{
    [Id(0)]
    public string RiderId { get; set; }
    
    [Id(1)]
    public string OrderId { get; set; }
    
    [Id(2)]
    public DateTime DeliveryStart {get; set;}
    
    [Id(3)]
    public DateTime? DeliveryEnd {get; set;}
}