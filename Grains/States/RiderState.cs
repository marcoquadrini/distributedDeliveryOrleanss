namespace Grains.States;

[GenerateSerializer]
public class RiderState
{
    [Id(0)]
    public string Name { get; set; }
    
    [Id(1)]
    public string LastName { get; set; }
    
    [Id(2)]
    public bool IsWorking { get; set; } = false;
    
    [Id(3)]
    public bool IsAvailable { get; set; } = true;
    
    [Id(4)]
    public string? AssignedOrder{ get; set; }

    public RiderState(string name, string lastName, bool isWorking)
    {
        Name = name;
        LastName = lastName;
        IsWorking = isWorking;
        if (isWorking) IsAvailable = true;
        else IsAvailable = false;
        AssignedOrder = null;
    }
}