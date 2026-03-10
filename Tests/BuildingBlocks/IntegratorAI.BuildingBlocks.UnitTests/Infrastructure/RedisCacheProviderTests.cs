using System.Text.Json;
using IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;
using NSubstitute;
using StackExchange.Redis;

namespace IntegratorAI.BuildingBlocks.UnitTests.Infrastructure;

public class RedisCacheProviderTests
{
    private record TestPayload(string Name, int Value);

    private readonly IDatabase _database = Substitute.For<IDatabase>();
    private readonly RedisCacheSettings _settings = new()
    {
        ConnectionString = "localhost",
        DefaultExpiration = TimeSpan.FromMinutes(5)
    };
    private readonly RedisCacheProvider _provider;

    public RedisCacheProviderTests()
    {
        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        multiplexer
            .GetDatabase(Arg.Any<int>(), Arg.Any<object?>())
            .Returns(_database);

        _provider = new RedisCacheProvider(multiplexer, _settings);
    }

    [Fact]
    public async Task GetAsync_ReturnsDeserializedValue_WhenKeyExists()
    {
        var payload = new TestPayload("test", 42);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        _database
            .StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns((RedisValue)bytes);

        var result = await _provider.GetAsync<TestPayload>("key");

        Assert.NotNull(result);
        Assert.Equal(payload, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsDefault_WhenKeyNotFound()
    {
        _database
            .StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(RedisValue.Null);

        var result = await _provider.GetAsync<TestPayload>("missing-key");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_PassesCorrectKey_ToDatabase()
    {
        const string key = "my-cache-key";
        _database
            .StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(RedisValue.Null);

        await _provider.GetAsync<TestPayload>(key);

        await _database.Received(1).StringGetAsync(key, Arg.Any<CommandFlags>());
    }

    [Fact]
    public async Task SetAsync_StoresSerializedValue_InDatabase()
    {
        var payload = new TestPayload("stored", 99);
        var expectedBytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        await _provider.SetAsync("key", payload);

        var call = _database.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "StringSetAsync");
        Assert.Equal(expectedBytes, actual: (byte[])(RedisValue)call.GetArguments()[1]!);
    }

    [Fact]
    public async Task SetAsync_UsesDefaultExpiration_WhenTtlNotProvided()
    {
        await _provider.SetAsync("key", new TestPayload("x", 1));

        var call = _database.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "StringSetAsync");
        Assert.Equal((Expiration)_settings.DefaultExpiration, (Expiration)call.GetArguments()[2]!);
    }

    [Fact]
    public async Task SetAsync_UsesProvidedTtl_WhenSpecified()
    {
        var ttl = TimeSpan.FromSeconds(30);

        await _provider.SetAsync("key", new TestPayload("x", 1), ttl);

        var call = _database.ReceivedCalls().Single(c => c.GetMethodInfo().Name == "StringSetAsync");
        Assert.Equal((Expiration)ttl, (Expiration)call.GetArguments()[2]!);
    }

    [Fact]
    public async Task RemoveAsync_CallsKeyDeleteAsync_WithCorrectKey()
    {
        const string key = "key-to-remove";
        _database
            .KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(true);

        await _provider.RemoveAsync(key);

        await _database.Received(1).KeyDeleteAsync(key, Arg.Any<CommandFlags>());
    }
}
