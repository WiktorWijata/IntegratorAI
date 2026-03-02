using IntegratorAI.BuildingBlocks.Infrastructure;
using IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;
using IntegratorAI.Chat.Infrastructure;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisCacheSettings>()
    ?? throw new InvalidOperationException("Redis configuration is missing.");
builder.Services.UseRedisAsDefaultCacheProvider(redisSettings);

var providersConfiguration = builder.Configuration.GetSection("Providers").Get<List<ProviderConfiguration>>() ?? [];
var connectionString = builder.Configuration.GetConnectionString("IntegratorAI");
builder.Services.AddProviders(connectionString!, providersConfiguration);
builder.Services.AddChat(connectionString!);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
