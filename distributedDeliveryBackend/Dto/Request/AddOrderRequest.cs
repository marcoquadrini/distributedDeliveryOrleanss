using Swashbuckle.AspNetCore.Annotations;

namespace distributedDeliveryBackend.Dto.Request
{
    public class AddOrderRequest
    {
        public string IdCustomer { get; set; } 
        
        public DeliveryInfo DeliveryDetails { get; set; } 
        
        public List<string> IdArticles { get; set; }
        
        [SwaggerSchema(ReadOnly = true)]
        public string? IdOrder { get; set; }

        public class DeliveryInfo
        {
            public string Name { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }

            public DeliveryInfo(string name, string lastName, string address, string city, string zipCode)
            {
                Name = name;
                LastName = lastName;
                Address = address;
                City = city;
                ZipCode = zipCode;
            }
            
        }
        
    }
}