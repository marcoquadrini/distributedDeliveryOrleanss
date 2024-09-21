using StackExchange.Redis;
using Constants = Grains.Utils.Constants;

namespace Grains.Services;

public class PendingDeliveriesService
{
    private readonly IDatabase _redis;

    public PendingDeliveriesService(IConnectionMultiplexer redisConnection)
    {
        _redis = redisConnection.GetDatabase();
    }

    public async Task<List<string>> GetPendingDeliveriesAsync()
    {
        var riderIds = await _redis.SetMembersAsync(Constants.RedisPendingDeliveriesKey);
        return riderIds.Select(r => (string)r).ToList();
    }
}