﻿using StackExchange.Redis;
using Constants = Grains.Utils.Constants;

namespace Grains.Services;

public class RiderAvailabilityService
{
    private readonly IDatabase _redis;

    public RiderAvailabilityService(IConnectionMultiplexer redisConnection)
    {
        _redis = redisConnection.GetDatabase();
    }

    public async Task<List<string>> GetAvailableRiderIdsAsync()
    {
        var riderIds = await _redis.SetMembersAsync(Constants.RedisAvailableRidersKey);
        return riderIds.Select(r => (string)r).ToList();
    }

}