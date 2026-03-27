using IntegratorAI.BuildingBlocks.Application.Behaviors;
using IntegratorAI.BuildingBlocks.Application.Caching;
using MediatR;
using NSubstitute;

namespace IntegratorAI.BuildingBlocks.UnitTests.Application;

public class CachingBehaviorTests
{
    private record CacheableRequest : IRequest<string>, ICacheable
    {
        public string CacheKey => "test-key";
        public TimeSpan? Ttl => TimeSpan.FromMinutes(5);
    }

    private record NonCacheableRequest : IRequest<string>;

    private readonly ICacheProvider _cacheProvider = Substitute.For<ICacheProvider>();
    private readonly CachingBehavior<CacheableRequest, string> _behavior;

    public CachingBehaviorTests()
    {
        _behavior = new CachingBehavior<CacheableRequest, string>(_cacheProvider);
    }

    [Fact]
    public async Task Handle_CallsNext_WhenRequestIsNotICacheable()
    {
        var behavior = new CachingBehavior<NonCacheableRequest, string>(_cacheProvider);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ => { nextCalled = true; return Task.FromResult("response"); };

        await behavior.Handle(new NonCacheableRequest(), next, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_DoesNotCheckCache_WhenRequestIsNotICacheable()
    {
        var behavior = new CachingBehavior<NonCacheableRequest, string>(_cacheProvider);
        RequestHandlerDelegate<string> next = _ => Task.FromResult("response");

        await behavior.Handle(new NonCacheableRequest(), next, CancellationToken.None);

        await _cacheProvider.DidNotReceive().GetAsync<string>(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ReturnsCachedValue_WhenCacheHit()
    {
        _cacheProvider.GetAsync<string>("test-key").Returns("cached-response");
        RequestHandlerDelegate<string> next = _ => Task.FromResult("fresh-response");

        var result = await _behavior.Handle(new CacheableRequest(), next, CancellationToken.None);

        Assert.Equal("cached-response", result);
    }

    [Fact]
    public async Task Handle_DoesNotCallNext_WhenCacheHit()
    {
        _cacheProvider.GetAsync<string>("test-key").Returns("cached-response");
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ => { nextCalled = true; return Task.FromResult("fresh"); };

        await _behavior.Handle(new CacheableRequest(), next, CancellationToken.None);

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Handle_CallsNext_WhenCacheMiss()
    {
        _cacheProvider.GetAsync<string>("test-key").Returns((string?)null);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = _ => { nextCalled = true; return Task.FromResult("response"); };

        await _behavior.Handle(new CacheableRequest(), next, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_StoresResultInCache_WhenCacheMiss()
    {
        _cacheProvider.GetAsync<string>("test-key").Returns((string?)null);
        RequestHandlerDelegate<string> next = _ => Task.FromResult("fresh-response");

        await _behavior.Handle(new CacheableRequest(), next, CancellationToken.None);

        await _cacheProvider.Received(1).SetAsync("test-key", "fresh-response", TimeSpan.FromMinutes(5));
    }
}
