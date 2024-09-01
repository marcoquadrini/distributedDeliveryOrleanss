namespace Grains.States;

public class CustomerState
{
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public List<string> Orders { get; set; } = new List<string>();
}