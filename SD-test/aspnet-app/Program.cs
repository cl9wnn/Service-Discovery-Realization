using Consul;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Consul Registration
var consulClient = new ConsulClient(c => c.Address = new Uri("http://consul:8500"));
var registration = new AgentServiceRegistration()
{
    ID = $"my-service-{Guid.NewGuid()}",
    Name = "my-service",
    Address = Dns.GetHostName(),
    Port = 80,
    Check = new AgentServiceCheck()
    {
        HTTP = $"http://{Dns.GetHostName()}:80/health",
        Interval = TimeSpan.FromSeconds(30),
        Timeout = TimeSpan.FromSeconds(5),
        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
    }
};

await consulClient.Agent.ServiceRegister(registration);

app.Lifetime.ApplicationStopping.Register(async () => 
{
    await consulClient.Agent.ServiceDeregister(registration.ID);
});

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();