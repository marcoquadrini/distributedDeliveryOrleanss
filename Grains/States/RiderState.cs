namespace Grains.States;

public class RiderState
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public bool IsWorking { get; set; } = false;
    public bool IsAvailable { get; set; } = true;
    public string AssignedOrders{ get; set; }
}