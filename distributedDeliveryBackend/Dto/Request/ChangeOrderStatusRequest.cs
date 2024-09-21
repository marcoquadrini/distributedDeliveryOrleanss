namespace distributedDeliveryBackend.Dto.Request;

public class ChangeOrderStatusRequest
{
    public string IdOrder { get; set; }
    
    public OrderStatus NewOrderState { get; set; }
}