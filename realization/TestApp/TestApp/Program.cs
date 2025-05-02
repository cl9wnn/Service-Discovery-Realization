using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("ServiceRegistry", client =>
{
    var sdUrl = builder.Configuration["SD_URL"] ?? "http://localhost:5100";
    client.BaseAddress = new Uri(sdUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ServiceRegistry");
    
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5103";
    var host = Environment.GetEnvironmentVariable("SERVICE_HOST") ?? "localhost";
    var serviceData = new 
    {
        Id = Guid.NewGuid(),
        Area = "weatherforecast",
        Port = int.Parse(port),
        Host = host
    };
    
    try
    {
        var response = await httpClient.PostAsJsonAsync("/services", serviceData);
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Ошибка регистрации приложения: {response.StatusCode}", response.StatusCode);
            Environment.Exit(1);
        }
        else
        {
            logger.LogInformation("Сервис успешно зарегистрирован");
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Критическая ошибка при регистрации сервиса");
        Environment.Exit(1);
    }
}

app.MapGet("/health", (ILogger<Program> logger) =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5103";
    logger.LogInformation($"Запрос отработал на {port}");
    return Results.Ok("healthy!");
});

app.Run();