using API.ServiceRegistry;

namespace API.Features.Services.DeleteServiceArea;

public class DeleteService
{
    public static async Task<IResult> Handle(Guid id, HttpContext context, IServiceRegistry registry,
        ILogger<DeleteService> logger)
    {
        logger.LogInformation("Начало обработки запроса на удаление сервиса.");

        if (id == Guid.Empty)
        {
            logger.LogWarning("Некорректный или отсутствующий GUID.");
            return Results.BadRequest("Некорректный или отсутствующий GUID.");
        }

        var service = await registry.GetByIdAsync(id);

        if (service == null)
        {
            logger.LogWarning("Сервис с таким айди не зарегистрирован!");
            return Results.BadRequest("Сервис с таким айди не зарегистрирован!");
        }

        logger.LogInformation("Запрос успешно прочитан.");

        try
        {
            logger.LogInformation("Удаление реплики...");
            await registry.UnregisterAsync(id);
            logger.LogInformation("Реплика успешно удалена.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении реплики.");
            return Results.StatusCode(500);
        }

        logger.LogInformation("Запрос на удаление реплики успешно обработан.");
        return Results.Ok();
    }
}
