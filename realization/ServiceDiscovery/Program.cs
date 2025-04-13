using API;
using API.Middlewares;
using API.ServiceRegistry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IServiceRegistry, InMemoryServiceRegistry>();
builder.Services.AddMappings();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/services")
    .MapServicesApi()
    .WithTags("Public");

app.UseGlobalExceptionHandler();
app.UseMiddleware<ProxyMiddleware>();

app.Run();
