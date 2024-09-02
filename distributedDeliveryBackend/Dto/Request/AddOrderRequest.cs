namespace distributedDeliveryBackend.Dto;

public class AddOrderRequest
{
    public string name { get; set; }
    public string lastname { get; set; }
    public string address { get; set; }
    public string city { get; set; }
    public string zipCode { get; set; }
    public string idArticle { get; set; }
  
}