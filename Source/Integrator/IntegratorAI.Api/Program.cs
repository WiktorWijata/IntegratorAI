using IntegratorAI.Chat.Infrastructure;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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
