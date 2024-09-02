namespace distributedDeliveryBackend.Dto;

public class ChangeOrderStatusRequest
{
    public int idOrder { get; set; }
    
    public OrderStatus newOrderState { get; set; }
}