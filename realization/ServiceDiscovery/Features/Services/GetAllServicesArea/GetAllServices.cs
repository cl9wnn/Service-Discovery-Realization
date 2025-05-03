using API.ServiceRegistry;
using AutoMapper;

namespace API.Features.Services.GetAllServicesArea;

public class GetAllServices
{
    public static async Task<IResult> Handle(string area,
        HttpContext context, IServiceRegistry registry, IMapper mapper, ILogger<GetAllServices> logger)
    {
        logger.LogInformation($"Получен запрос на получение всех сервисов в: {area}");

        if (string.IsNullOrEmpty(area))
        {
            logger.LogWarning("Область сервиса не указана в запросе.");
            return Results.BadRequest("Не указана область сервиса");
        }

        logger.LogInformation($"Выполняется поиск сервисов в области: {area}");

        var services = await registry.TryGetByAreaAsync(area);

        if (services == null || services.Count == 0)
        {
            logger.LogWarning($"Сервисы не найдены в области: {area}");
            return Results.NotFound("Сервисы не найдены");
        }

        logger.LogInformation($"Найдено {services.Count} сервисов в области: {area}");

        var response = mapper.Map<GetAllServicesResponse>(services);

        logger.LogInformation("Возврат результатов клиенту.");

        return Results.Ok(response);
    }
}
