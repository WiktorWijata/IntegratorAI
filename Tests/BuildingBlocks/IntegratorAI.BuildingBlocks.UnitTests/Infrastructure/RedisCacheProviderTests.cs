using System.Text.Json;
using IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;
using Moq;
using StackExchange.Redis;

namespace IntegratorAI.BuildingBlocks.UnitTests.Infrastructure;

public class RedisCacheProviderTests
{
    private record TestPayload(string Name, int Value);

    private readonly Mock<IDatabase> _databaseMock = new();
    private readonly RedisCacheSettings _settings = new()
    {
        ConnectionString = "localhost",
        DefaultExpiration = TimeSpan.FromMinutes(5)
    };
    private readonly RedisCacheProvider _provider;

    public RedisCacheProviderTests()
    {
        var multiplexerMock = new Mock<IConnectionMultiplexer>();
        multiplexerMock
            .Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(_databaseMock.Object);

        _provider = new RedisCacheProvider(multiplexerMock.Object, _settings);
    }

    [Fact]
    public async Task GetAsync_ReturnsDeserializedValue_WhenKeyExists()
    {
        var payload = new TestPayload("test", 42);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        _databaseMock
            .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)bytes);

        var result = await _provider.GetAsync<TestPayload>("key");

        Assert.NotNull(result);
        Assert.Equal(payload, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsDefault_WhenKeyNotFound()
    {
        _databaseMock
            .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var result = await _provider.GetAsync<TestPayload>("missing-key");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_PassesCorrectKey_ToDatabase()
    {
        const string key = "my-cache-key";
        _databaseMock
            .Setup(d => d.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        await _provider.GetAsync<TestPayload>(key);

        _databaseMock.Verify(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()), Times.Once());
    }

    [Fact]
    public async Task SetAsync_StoresSerializedValue_InDatabase()
    {
        var payload = new TestPayload("stored", 99);
        var expectedBytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        await _provider.SetAsync("key", payload);

        var invocation = _databaseMock.Invocations.Single(i => i.Method.Name == "StringSetAsync");
        Assert.Equal(expectedBytes, (byte[])(RedisValue)invocation.Arguments[1]!);
    }

    [Fact]
    public async Task SetAsync_UsesDefaultExpiration_WhenTtlNotProvided()
    {
        await _provider.SetAsync("key", new TestPayload("x", 1));

        var invocation = _databaseMock.Invocations.Single(i => i.Method.Name == "StringSetAsync");
        Assert.Equal((Expiration)_settings.DefaultExpiration, (Expiration)invocation.Arguments[2]!);
    }

    [Fact]
    public async Task SetAsync_UsesProvidedTtl_WhenSpecified()
    {
        var ttl = TimeSpan.FromSeconds(30);

        await _provider.SetAsync("key", new TestPayload("x", 1), ttl);

        var invocation = _databaseMock.Invocations.Single(i => i.Method.Name == "StringSetAsync");
        Assert.Equal((Expiration)ttl, (Expiration)invocation.Arguments[2]!);
    }

    [Fact]
    public async Task RemoveAsync_CallsKeyDeleteAsync_WithCorrectKey()
    {
        const string key = "key-to-remove";
        _databaseMock
            .Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await _provider.RemoveAsync(key);

        _databaseMock.Verify(d => d.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once());
    }
}
