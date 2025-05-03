using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ApiGateway;

public class LoadBalancingMiddleware(
    RequestDelegate next,
    ILogger<LoadBalancingMiddleware> logger,
    IHttpClientFactory httpClientFactory,
    IBalancer balancer)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;
        var segments = path.Split('/');
        var area = segments.FirstOrDefault();
        var client = httpClientFactory.CreateClient();

        if (string.IsNullOrEmpty(area))
        {
            await next(context);
            return;
        }

        var remainingPath = string.Join('/', segments.Skip(1));

        var sdUrl = Environment.GetEnvironmentVariable("SD_URL") ?? "http://localhost:5100";
        var requestUrl =  sdUrl + "/services" + $"/{area}";
        var sdResponse = await GetAvailableServicesAsync(client, requestUrl, context.RequestAborted);

        if (sdResponse is null || sdResponse.Services.Count == 0)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Нет доступных сервисов");
            return;
        }

        var selectedService = balancer.GetNextService(sdResponse.Services);
        if (selectedService is null)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Нет доступных сервисов");
            return;
        }
        
        var targetUri = new UriBuilder
        {
            Scheme = "http",
            Host = selectedService.Host,
            Port = selectedService.Port,
            Path = remainingPath,
            Query = context.Request.QueryString.Value,
        }.Uri;

        logger.LogInformation($"Перенаправление запроса на {targetUri}");

        await ProxyRequestAsync(client, context, targetUri);
    }

    private async Task<ServiceDiscoveryResponse?> GetAvailableServicesAsync(HttpClient client, string serviceDiscoveryUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetAsync(serviceDiscoveryUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<ServiceDiscoveryResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении списка сервисов");
            return null;
        }
    }

    private async Task ProxyRequestAsync(HttpClient client, HttpContext context, Uri targetUri)
    {
        try
        {
            var requestMessage = new HttpRequestMessage();

            requestMessage.RequestUri = targetUri;
            requestMessage.Method = new HttpMethod(context.Request.Method);

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            if (context.Request.ContentLength > 0)
            {
                requestMessage.Content = new StreamContent(context.Request.Body);
                foreach (var header in context.Request.Headers)
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            using var responseMessage = await client.SendAsync(
                requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            await using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            await responseStream.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsync($"{ex.Message}");
        }
    }
}