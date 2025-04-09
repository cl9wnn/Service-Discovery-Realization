using Consul;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

var consulClient = new ConsulClient(c => c.Address = new Uri("http://consul:8500"));
var registration = new AgentServiceRegistration()
{
    ID = $"my-service-{Guid.NewGuid()}",
    Name = "my-service",
    Address = GetLocalIpAddress(),
    Port = 80,
    Check = new AgentServiceCheck()
    {
        HTTP = $"http://{Dns.GetHostName()}:80/health",
        Interval = TimeSpan.FromSeconds(30),
        Timeout = TimeSpan.FromSeconds(5),
        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(2)
    }
};

await consulClient.Agent.ServiceRegister(registration);

app.Lifetime.ApplicationStopping.Register(async () => 
{
    await consulClient.Agent.ServiceDeregister(registration.ID);
});

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Use(async (context, next) => {
    context.Response.Headers.Append("X-Service-Id", registration.ID);
    await next();
});

app.Run();
return;

string GetLocalIpAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return ip.ToString();
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}