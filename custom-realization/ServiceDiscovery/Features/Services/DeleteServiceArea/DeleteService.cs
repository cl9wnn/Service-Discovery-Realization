using API.ServiceRegistry;

namespace API.Features.Services.DeleteServiceArea;

public class DeleteService
{
    public static async Task<IResult> Handle(Guid id, HttpContext context, IServiceRegistry registry,
        ILogger<DeleteService> logger)
    {
        var correlationId = context.Items[AppConstants.CorrelationIdHeader]!;

        logger.LogInformation("{@correlationId}: Начало обработки запроса на удаление сервиса.", correlationId);

        if (id == Guid.Empty)
        {
            logger.LogWarning("{@correlationId}: Некорректный или отсутствующий GUID.", correlationId);
            return Results.BadRequest("Некорректный или отсутствующий GUID.");
        }

        var service = await registry.GetByIdAsync(id);

        if (service == null)
        {
            logger.LogWarning("{@correlationId}: Сервис с таким айди не зарегистрирован!", correlationId);
            return Results.BadRequest("Сервис с таким айди не зарегистрирован!");
        }

        logger.LogInformation("{@correlationId}: Запрос успешно прочитан.", correlationId);

        try
        {
            logger.LogInformation("{@correlationId}: Удаление реплики...", correlationId);
            await registry.UnregisterAsync(id);
            logger.LogInformation("{@correlationId}: Реплика успешно удалена.", correlationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{@correlationId}: Ошибка при удалении реплики.", correlationId);
            return Results.StatusCode(500);
        }

        logger.LogInformation("{@correlationId}: Запрос на удаление реплики успешно обработан.", correlationId);
        return Results.Ok();
    }
}
