namespace distributedDeliveryBackend.Dto;

public class SetWorkingRiderRequest
{
    public string RiderId { set; get; }
    
    public bool IsWorking { set; get; }

    public SetWorkingRiderRequest(string riderId, bool isWorking)
    {
        RiderId = riderId;
        IsWorking = isWorking;
    }
}