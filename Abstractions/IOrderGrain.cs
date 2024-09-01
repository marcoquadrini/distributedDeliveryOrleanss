using Microsoft.CodeAnalysis;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstractions
{
    public interface IOrderGrain : IGrainWithStringKey
    {
        Task<string> GetItemList();

        Task AddItem(String item);

        Task SetLocation(Location location);

        Task<Location> GetLocation();
        
        Task<string> GetStatus();
        
        Task AssignToRider(string riderId);

        Task SetProducts(List<string> productIds);

        Task UpdateStatus(string status);
    }
}
