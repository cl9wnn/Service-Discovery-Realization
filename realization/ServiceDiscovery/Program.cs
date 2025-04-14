using API;
using API.HealthChecker;
using API.ServiceRegistry;
using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IServiceRegistry, InMemoryServiceRegistry>();
builder.Services.AddSingleton<IHealthChecker, HealthChecker>();
builder.Services.AddMappings();
builder.Services.AddHttpClient();
builder.Services.AddLogging();

builder.Services.AddHangfire(config => config.UseMemoryStorage());

builder.Services.AddHangfireServer();

var app = builder.Build();
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
