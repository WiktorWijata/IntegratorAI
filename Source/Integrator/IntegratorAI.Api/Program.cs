using Microsoft.AspNetCore.HttpOverrides;
using IntegratorAI.Api.Middleware;
using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Infrastructure;
using IntegratorAI.Context.Infrastructure;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Infrastructure;
using RescuePC.Software.Caching;
using RescuePC.Software.Caching.Providers.Redis;
using RescuePC.Software.Logging.Providers.Serilog;
using RescuePC.Software.Telemetry;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilog();

var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisCacheSettings>()
    ?? throw new InvalidOperationException("Redis configuration is missing.");
builder.Services.AddRedisAsDefaultCacheProvider(redisSettings);

var connectionString = builder.Configuration.GetConnectionString("IntegratorAI");
var providersConfiguration = builder.Configuration.GetSection("Providers").Get<List<ProviderConfiguration>>() ?? [];
builder.Services.AddProviders(connectionString!, providersConfiguration);

var chatSettings = builder.Configuration.GetSection("Chat").Get<ChatSettings>() ?? new ChatSettings();
builder.Services.AddChat(connectionString!, chatSettings);
builder.Services.AddContext(connectionString!);
builder.Services.AddOpenTelemetryInstrumentation();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UsePathBase("/integratorai/api");
app.UseRouting();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler();
app.UseAuthorization();
app.MapControllers();
app.Run();
