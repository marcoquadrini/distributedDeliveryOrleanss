namespace distributedDeliveryBackend.Dto
{
    public class AddOrderRequest
    {
        public string idCustomer { get; set; } 
        
        public DeliveryInfo deliveryDetails { get; set; } 
        
        public List<string> idArticles { get; set; }

        public class DeliveryInfo
        {
            public string name { get; set; }
            public string lastName { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string zipCode { get; set; }

            public DeliveryInfo(string name, string lastName, string address, string city, string zipCode)
            {
                this.name = name;
                this.lastName = lastName;
                this.address = address;
                this.city = city;
                this.zipCode = zipCode;
            }
            
        }
        
    }
}