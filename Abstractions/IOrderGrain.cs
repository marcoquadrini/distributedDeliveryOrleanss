using Microsoft.CodeAnalysis;


namespace Abstractions
{
    public interface IOrderGrain : IGrainWithStringKey
    {
        Task<string> GetItemList();

        Task AddItem(String item);
        
        Task<string> GetStatus();
        
        Task AssignToRider(string riderId);

        Task SetProducts(List<string> productIds);

        Task<bool> UpdateStatus(string status);
    }
    
}
