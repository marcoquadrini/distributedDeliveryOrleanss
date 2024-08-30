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
        Task<String> GetItemList();

        Task AddItem(String item);

        Task SetLocation(Location location);

        Task<Location> GetLocation();

    }
}
