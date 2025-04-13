var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Динамически подтягивать порт SD
builder.Services.AddHttpClient("ServiceRegistry", client =>
{
    client.BaseAddress = new Uri("http://localhost:5100"); // Адрес SD
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
    
    // TODO: Динамически подтягивать порт приложения
    var serviceData = new 
    {
        Id = Guid.NewGuid(),
        Area = "weatherforecast",
        Port = 5101
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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/health", () => StatusCodes.Status200OK);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}