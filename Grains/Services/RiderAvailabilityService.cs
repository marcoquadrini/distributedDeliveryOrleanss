using StackExchange.Redis;
using Constants = Grains.Utils.Constants;

namespace Grains.Services;


/// <summary>
/// Manages the currently available riders in Redis DB
/// </summary>
public class RiderAvailabilityService
{
    private readonly IDatabase _redis;

    public RiderAvailabilityService(IConnectionMultiplexer redisConnection)
    {
        _redis = redisConnection.GetDatabase();
    }

    /// <summary>
    /// Gets the currently available riders stored in Redis DB
    /// </summary>
    /// <returns>list of the available riders ids</returns>
    public async Task<List<string>> GetAvailableRiderIdsAsync()
    {
        var riderIds = await _redis.SetMembersAsync(Constants.RedisAvailableRidersKey);
        return riderIds.Select(r => (string)r).ToList();
    }

    /// <summary>
    /// Adds a rider to the available riders set in Redis.
    /// </summary>
    /// <param name="riderId"></param>
    public async Task AddAvailableRiderAsync(string riderId)
    {
        await _redis.SetAddAsync(Constants.RedisAvailableRidersKey, riderId);
    }

    /// <summary>
    /// Removes a rider from the available riders set in Redis.
    /// </summary>
    /// <param name="riderId"></param>
    public async Task RemoveAvailableRiderAsync(string riderId)
    {
        await _redis.SetRemoveAsync(Constants.RedisAvailableRidersKey, riderId);
    }
}