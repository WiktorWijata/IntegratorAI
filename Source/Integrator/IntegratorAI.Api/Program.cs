using IntegratorAI.BuildingBlocks.Infrastructure;
using IntegratorAI.BuildingBlocks.Infrastructure.Caching.Redis;
using IntegratorAI.Api.Middleware;
using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Infrastructure;
using IntegratorAI.Context.Infrastructure;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisCacheSettings>()
    ?? throw new InvalidOperationException("Redis configuration is missing.");
builder.Services.UseRedisAsDefaultCacheProvider(redisSettings);

var connectionString = builder.Configuration.GetConnectionString("IntegratorAI");
var providersConfiguration = builder.Configuration.GetSection("Providers").Get<List<ProviderConfiguration>>() ?? [];
builder.Services.AddProviders(connectionString!, providersConfiguration);

var chatSettings = builder.Configuration.GetSection("Chat").Get<ChatSettings>() ?? new ChatSettings();
builder.Services.AddChat(connectionString!, chatSettings);
builder.Services.AddContext(connectionString!);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
