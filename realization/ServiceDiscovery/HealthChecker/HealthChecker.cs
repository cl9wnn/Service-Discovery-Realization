using API.Models;
using API.ServiceRegistry;

namespace API.HealthChecker;

public class HealthChecker(
    IServiceRegistry serviceRegistry,
    IHttpClientFactory httpClientFactory,
    ILogger<HealthChecker> logger)
    : IHealthChecker
{
    public async Task CheckAllServicesAsync()
    {
        try
        {
            var allServices = await serviceRegistry.GetAllAsync();
            var tasks = allServices.Select(CheckServiceHealthAsync);
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при просмотре реплик");
        }
    }

    private async Task CheckServiceHealthAsync(ServiceInfo service)
    {
        var traceId = Guid.NewGuid();
        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add(AppConstants.CorrelationIdHeader, traceId.ToString());

            logger.LogInformation("{@TraceId}: Начинается проверка реплики http://{@Host}:{@Port}/health", traceId, service.Host, service.Port);
            var healthCheckUrl = $"http://{service.Host}:{service.Port}/health";
            var response = await client.GetAsync(healthCheckUrl);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("{@TraceId}: Сервис {@Id} на порту {@Port} неработоспособен. Код: {@StatusCode}", traceId, service.Id, service.Port, response.StatusCode);
                await serviceRegistry.DeleteAsync(service.Id);
            }

            logger.LogInformation("{@TraceId}: Сервис {@Id} на порту {@Port} успешно работает. Код: {@StatusCode}", traceId, service.Id, service.Port, response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,"{TraceId}: Ошибка проверки работоспособности сервиса {Id} на порту {Port}", traceId, service.Id, service.Port);

            await serviceRegistry.DeleteAsync(service.Id);
        }
    }
}
