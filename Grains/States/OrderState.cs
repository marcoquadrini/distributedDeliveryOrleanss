namespace Grains.States;

public class OrderState
{
    public string id { get; set; }
    public List<string> ProductIds { get; set; } = new();
    public string Status { get; set; } = "Pending";
    public string RiderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }

    public override string ToString()
    {
        string productList = ProductIds != null && ProductIds.Any() 
            ? string.Join(", ", ProductIds) 
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