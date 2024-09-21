using StackExchange.Redis;

namespace Grains.Services;

public class RiderAvailabilityService
{
    private readonly IDatabase _redis;
    private const string RedisAvailableRidersKey = "available_riders";

    public RiderAvailabilityService(IConnectionMultiplexer redisConnection)
    {
        _redis = redisConnection.GetDatabase();
    }

    public async Task<List<string>> GetAvailableRiderIdsAsync()
    {
        var riderIds = await _redis.SetMembersAsync(RedisAvailableRidersKey);
        return riderIds.Select(r => (string)r).ToList();
    }
}