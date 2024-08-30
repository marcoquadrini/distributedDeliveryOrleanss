using Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains
{
    public class OrderGrain : Orleans.Grain, IOrderGrain
    {
        private readonly ILogger logger;
        private String customerName;
        private String customerSurname;

        public OrderGrain(ILogger<OrderGrain> logger, String customerName, String customerSurname)
        {
            this.logger = logger;
            this.customerName = customerName;
            this.customerSurname = customerSurname;
        }

        public Task AddItem(string item)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetItemList()
        {
            throw new NotImplementedException();
        }

        public Task<Location> GetLocation()
        {
            throw new NotImplementedException();
        }

        public Task SetLocation(Location location)
        {
            throw new NotImplementedException();
        }
    }
}
