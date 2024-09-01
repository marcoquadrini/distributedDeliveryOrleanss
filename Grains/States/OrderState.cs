namespace Grains.States;

public class OrderState
{
    public List<string> ProductIds { get; set; } = new();
    public string Status { get; set; }
    public string RiderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
}