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

            // TODO: хардкод
            var healthCheckUrl = $"http://localhost:{service.Port}/health";
            var response = await client.GetAsync(healthCheckUrl);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning($"Сервис {service.Id} на порту {service.Port} неработоспособен. Код статуса: {response.StatusCode}");
                await serviceRegistry.UnregisterAsync(service.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                $"Ошибка проверки работоспособности сервиса {service.Id} на порту {service.Port}");
            await serviceRegistry.UnregisterAsync(service.Id);
        }
    }
}
