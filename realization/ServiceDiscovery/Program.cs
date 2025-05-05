using API;
using API.HealthChecker;
using API.Middlewares;
using API.ServiceRegistry;
using Hangfire;
using Hangfire.MemoryStorage;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IHealthChecker, HealthChecker>();
builder.Services.AddMappings();
builder.Services.AddHttpClient();
builder.Services.AddLogging();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        _ => ConnectionMultiplexer.Connect("redis:6379"));
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IServiceRegistry, InMemoryServiceRegistry>();
}
else
{
    builder.Services.AddSingleton<IServiceRegistry, RedisServiceRegistry>();
}

builder.Services.AddHangfire(config => config.UseMemoryStorage());

builder.Services.AddHangfireServer();

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseHangfireDashboard();

RecurringJob.AddOrUpdate<IHealthChecker>(
    "health-check", x => x.CheckAllServicesAsync(), Cron.Minutely);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/services")
    .MapServicesApi()
    .WithTags("Public");

app.UseGlobalExceptionHandler();

app.Run();
