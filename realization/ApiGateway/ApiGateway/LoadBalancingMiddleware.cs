using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ApiGateway;

public class LoadBalancingMiddleware(
    RequestDelegate next,
    ILogger<LoadBalancingMiddleware> logger,
    IHttpClientFactory httpClientFactory,
    IBalancer balancer)
{
    private string _correlationId;
    
    public async Task InvokeAsync(HttpContext context)
    {
        _correlationId = context.Items[AppConstants.CorrelationIdHeader]!.ToString()!;
        
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
            context.Response.ContentType = "text/plain;charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync($"Нет доступных сервисов в области \"{area}\"");
            return;
        }

        var selectedService = balancer.GetNextService(sdResponse.Services);
        if (selectedService is null)
        {
            context.Response.ContentType = "text/plain;charset=utf-8";
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

        logger.LogInformation("{@CorrelationId}: Перенаправление запроса на {TargetUri}", _correlationId, targetUri);

        await ProxyRequestAsync(client, context, targetUri);
    }

    private async Task<ServiceDiscoveryResponse?> GetAvailableServicesAsync(HttpClient client, string serviceDiscoveryUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            client.DefaultRequestHeaders.Add(AppConstants.CorrelationIdHeader, _correlationId);
            var response = await client.GetAsync(serviceDiscoveryUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseServices = JsonSerializer.Deserialize<ServiceDiscoveryResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (responseServices?.Services == null)
            {
                return null;
            }
            
            var healthyServices = responseServices.Services
                .Where(s => s.IsHealthy)
                .ToList();
            
            return new ServiceDiscoveryResponse
            {
                Services = healthyServices,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{@CorrelationId}: Ошибка при получении списка сервисов", _correlationId);
            return null;
        }
    }

    private async Task ProxyRequestAsync(HttpClient client, HttpContext context, Uri targetUri)
    {
        try
        {
            var requestMessage = new HttpRequestMessage
            {
                RequestUri = targetUri,
                Method = new HttpMethod(context.Request.Method)
            };

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            requestMessage.Headers.Add(AppConstants.CorrelationIdHeader, _correlationId);
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

            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogWarning("{@TraceId}: Целевой сервис не поддерживает путь {@TargetUri} (404)", _correlationId, targetUri);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "text/plain;charset=utf-8";
                await context.Response.WriteAsync("Маршрут не найден в целевом сервисе");
                return;
            }
            context.Response.StatusCode = (int)responseMessage.StatusCode;
            
            await using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            await responseStream.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "text/plain;charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsync($"{ex.Message}");
        }
    }
}