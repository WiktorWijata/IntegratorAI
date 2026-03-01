namespace IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;

public class RedisCacheSettings
{
    public required string ConnectionString { get; set; }
    public string? Password { get; set; }
    public TimeSpan DefaultExpiration { get; set; }
}
