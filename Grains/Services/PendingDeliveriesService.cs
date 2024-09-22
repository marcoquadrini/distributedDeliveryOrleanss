using StackExchange.Redis;
using Constants = Grains.Utils.Constants;

namespace Grains.Services;

/**
 * Manages the pending deliveries in Redis DB
 */
public class PendingDeliveriesService
{
    private readonly IDatabase _redis;

    public PendingDeliveriesService(IConnectionMultiplexer redisConnection)
    {
        _redis = redisConnection.GetDatabase();
    }

    /**
     * Gets all the current pending deliveries from Redis database
     */
    public async Task<List<string>> GetPendingDeliveriesAsync()
    {
        var riderIds = await _redis.SetMembersAsync(Constants.RedisPendingDeliveriesKey);
        return riderIds.Select(r => (string)r).ToList();
    }
}