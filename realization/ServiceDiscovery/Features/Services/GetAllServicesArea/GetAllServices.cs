using API.ServiceRegistry;
using AutoMapper;

namespace API.Features.Services.GetAllServicesArea;

public class GetAllServices
{
    public static async Task<IResult> Handle(string area,
        HttpContext context, IServiceRegistry registry, IMapper mapper, ILogger<GetAllServices> logger)
    {
        var correlationId = context.Items[AppConstants.CorrelationIdHeader]!;

        logger.LogInformation("{@correlationId}: Получен запрос на получение всех сервисов в: {area}", correlationId, area);

        if (string.IsNullOrEmpty(area))
        {
            logger.LogWarning("{@correlationId}: Область сервиса не указана в запросе.", correlationId);
            return Results.BadRequest("Не указана область сервиса");
        }

        logger.LogInformation("{@correlationId}: Выполняется поиск сервисов в области: {area}", correlationId, area);

        var services = await registry.TryGetByAreaAsync(area);

        if (services == null || services.Count == 0)
        {
            logger.LogWarning("{@correlationId}: Сервисы не найдены в области: {@area}", correlationId, area);
            return Results.NotFound("Сервисы не найдены");
        }

        logger.LogInformation("{@correlationId}: Найдено {@Count} сервисов в области: {@area}", correlationId, services.Count, area);

        var response = mapper.Map<GetAllServicesResponse>(services);

        logger.LogInformation("{@correlationId}: Возврат результатов клиенту.", correlationId);

        return Results.Ok(response);
    }
}
