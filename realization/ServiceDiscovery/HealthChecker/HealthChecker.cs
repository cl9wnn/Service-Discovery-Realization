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
        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var traceId = Guid.NewGuid();
            client.DefaultRequestHeaders.Add(AppConstants.CorrelationIdHeader, traceId.ToString());

            logger.LogInformation($"Начинается проверка реплики http://{service.Host}:{service.Port}/health");
            var healthCheckUrl = $"http://{service.Host}:{service.Port}/health";
            var response = await client.GetAsync(healthCheckUrl);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning($"Сервис {service.Id} на порту {service.Port} неработоспособен. Код: {response.StatusCode}");
                await serviceRegistry.UnregisterAsync(service.Id);
            }

            logger.LogInformation($"Сервис {service.Id} на порту {service.Port} успешно работает. Код: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                $"Ошибка проверки работоспособности сервиса {service.Id} на порту {service.Port}");
            await serviceRegistry.UnregisterAsync(service.Id);
        }
    }
}
