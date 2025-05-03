using API.Models;
using API.ServiceRegistry;
using AutoMapper;

namespace API.Features.Services.AddServiceArea;

public class AddService
{
    public static async Task<IResult> Handle(HttpContext context,
        IServiceRegistry registry, IMapper mapper, ILogger<AddService> logger)
    {
        logger.LogInformation("Начало обработки запроса на добавление сервиса.");

        var request = await context.Request.ReadFromJsonAsync<AddServiceRequest>();

        if (request is null)
        {
            logger.LogWarning("Не удалось прочитать тело запроса.");
            return Results.BadRequest("Некорректный формат запроса.");
        }

        logger.LogInformation("Запрос успешно прочитан.");

        var service = mapper.Map<ServiceInfo>(request);

        try
        {
            logger.LogInformation("Регистрация реплики...");
            await registry.RegisterAsync(service);
            logger.LogInformation("Реплика успешно зарегистрирована.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при регистрации реплики.");
            return Results.StatusCode(500);
        }

        logger.LogInformation("Запрос на добавление реплики успешно обработан.");
        return Results.Ok(service);
    }
}

