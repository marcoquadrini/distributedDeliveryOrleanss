namespace Grains.States;

[GenerateSerializer]
public class CustomerState
{
    [Id(0)]
    public int CustomerId { get; set; }
    
    [Id(1)]
    public string Name { get; set; }
    
    [Id(2)]
    public string LastName { get; set; }
    
    [Id(3)]
    public string Email { get; set; }
    
    [Id(4)]
    public List<string> Orders { get; set; } = new();
}