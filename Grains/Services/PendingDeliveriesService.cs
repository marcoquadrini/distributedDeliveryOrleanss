using StackExchange.Redis;
using Constants = Grains.Utils.Constants;

namespace Grains.Services;

/// <summary>
/// Manages the pending deliveries in Redis DB
/// </summary>
public class PendingDeliveriesService
{
    private readonly IDatabase _redis;

    public PendingDeliveriesService(IConnectionMultiplexer redisConnection)
    {
        _redis = redisConnection.GetDatabase();
    }
    
    /// <summary>
    /// Adds a delivery to the pending deliveries set in Redis.
    /// </summary>
    /// <param name="orderId"></param>
    public async Task AddPendingDeliveryAsync(string orderId)
    {
        await _redis.SetAddAsync(Constants.RedisPendingDeliveriesKey, orderId);
    }

    /// <summary>
    /// Removes a delivery from the pending deliveries set in Redis.
    /// </summary>
    /// <param name="orderId"></param>
    public async Task RemovePendingDeliveryAsync(string orderId)
    {
        await _redis.SetRemoveAsync(Constants.RedisPendingDeliveriesKey, orderId);
    }

    /// <summary>
    /// Manages the pending deliveries in Redis DB
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetPendingDeliveriesAsync()
    {
        var riderIds = await _redis.SetMembersAsync(Constants.RedisPendingDeliveriesKey);
        return riderIds.Select(r => (string)r).ToList();
    }
}