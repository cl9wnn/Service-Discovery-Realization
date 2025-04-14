using System.Text;

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

app.MapGet("/health", () => Results.Ok("healthy!"));

app.Run();