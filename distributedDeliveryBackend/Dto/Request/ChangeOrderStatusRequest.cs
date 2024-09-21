namespace distributedDeliveryBackend.Dto;

public class ChangeOrderStatusRequest
{
    public string idOrder { get; set; }
    
    public OrderStatus newOrderState { get; set; }
}