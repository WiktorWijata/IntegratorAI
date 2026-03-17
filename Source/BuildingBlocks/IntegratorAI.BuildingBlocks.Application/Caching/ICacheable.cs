namespace IntegratorAI.BuildingBlocks.Application.Caching;

public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan? Ttl { get; }
}
