using API.Models;
using API.ServiceRegistry;
using AutoMapper;

namespace API.Features.Services.AddServiceArea;

public class AddService
{
    public static async Task<IResult> Handle(HttpContext context,
        IServiceRegistry registry, IMapper mapper, ILogger<AddService> logger)
    {
        var correlationId = context.Items[AppConstants.CorrelationIdHeader]!;

        logger.LogInformation("{@correlationId}: Начало обработки запроса на добавление сервиса.", correlationId);

        var request = await context.Request.ReadFromJsonAsync<AddServiceRequest>();

        if (request is null)
        {
            logger.LogWarning("{@correlationId}: Не удалось прочитать тело запроса.", correlationId);
            return Results.BadRequest("Некорректный формат запроса.");
        }

        logger.LogInformation("{@correlationId}: Запрос успешно прочитан.", correlationId);

        var service = mapper.Map<ServiceInfo>(request);

        try
        {
            logger.LogInformation("{@correlationId}: Регистрация реплики...", correlationId);
            await registry.RegisterAsync(service);
            logger.LogInformation("{@correlationId}: Реплика успешно зарегистрирована.", correlationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{@correlationId}: Ошибка при регистрации реплики.", correlationId);
            return Results.StatusCode(500);
        }

        logger.LogInformation("{@correlationId}: Запрос на добавление реплики успешно обработан.", correlationId);
        return Results.Ok(service);
    }
}

