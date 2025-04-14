using ApiGateway;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IBalancer, RoundRobinBalancer>();
builder.Services.AddHttpClient();
builder.Services.Configure<ServiceDiscoveryOptions>(
    builder.Configuration.GetSection(nameof(ServiceDiscoveryOptions)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<LoadBalancingMiddleware>();

app.Run();