using API.ServiceRegistry;
using AutoMapper;

namespace API.Features.Services.GetAllServicesArea;

public class GetAllServices
{
    public static async Task<IResult> Handle(string area,
        HttpContext context, IServiceRegistry registry, IMapper mapper, ILogger<GetAllServices> logger)
    {
        var correlationId = context.Items[AppConstants.CorrelationIdHeader]!;

        logger.LogInformation($"{correlationId}: Получен запрос на получение всех сервисов в: {area}");

        if (string.IsNullOrEmpty(area))
        {
            logger.LogWarning($"{correlationId}: Область сервиса не указана в запросе.");
            return Results.BadRequest("Не указана область сервиса");
        }

        logger.LogInformation($"{correlationId}: Выполняется поиск сервисов в области: {area}");

        var services = await registry.TryGetByAreaAsync(area);

        if (services == null || services.Count == 0)
        {
            logger.LogWarning($"{correlationId}: Сервисы не найдены в области: {area}");
            return Results.NotFound("Сервисы не найдены");
        }

        logger.LogInformation($"{correlationId}: Найдено {services.Count} сервисов в области: {area}");

        var response = mapper.Map<GetAllServicesResponse>(services);

        logger.LogInformation($"{correlationId}: Возврат результатов клиенту.");

        return Results.Ok(response);
    }
}
