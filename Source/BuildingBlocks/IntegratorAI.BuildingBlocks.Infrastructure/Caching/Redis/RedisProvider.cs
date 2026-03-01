using System.Text.Json;
using IntegratorAI.BuildingBlocks.Application.Caching;
using StackExchange.Redis;

namespace IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;

public class RedisCacheProvider : ICacheProvider
{
    private readonly IDatabase _database;
    private readonly RedisCacheSettings _settings;

    public RedisCacheProvider(IConnectionMultiplexer multiplexer, RedisCacheSettings settings)
    {
        _database = multiplexer.GetDatabase();
        _settings = settings;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(((byte[])value)!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var serialized = JsonSerializer.SerializeToUtf8Bytes(value);
        await _database.StringSetAsync(key, serialized, ttl);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}
