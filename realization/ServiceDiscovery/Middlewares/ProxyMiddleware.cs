using API.ServiceRegistry;

namespace API.Middlewares;

public class ProxyMiddleware(
    RequestDelegate next,
    IHttpClientFactory httpClientFactory,
    IServiceRegistry serviceRegistry,
    ILogger<ProxyMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;
        var segments = path.Split('/');
        var area = path.Split('/').FirstOrDefault();

        // TODO: Хардкод
        if (string.IsNullOrEmpty(area) || area == "services")
        {
            await next(context);
            return;
        }

        var service = await serviceRegistry.TryGetByAreaAsync(area);

        if (service == null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            logger.LogWarning($"Нет доступных сервисов для: {area}");
            return;
        }

        var remainingPath = string.Join('/', segments.Skip(1));
        var targetUri = new UriBuilder
        {
            Scheme = "http",
            Host = "localhost",
            Port = service.Port,
            Path = remainingPath,
            Query = context.Request.QueryString.Value,
        }.Uri;

        logger.LogInformation($"Проксируем запрос на реплику {service.Id} порт {targetUri.Port}");

        try
        {
            var client = httpClientFactory.CreateClient();
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
            await responseStream.CopyToAsync(
                context.Response.Body,
                context.RequestAborted);
        }
        catch (Exception ex)
        {
            // TODO: Что делать с неработающими репликами
            logger.LogError(ex, $"Ошибка на реплике {service.Id} at port {service.Port}");

            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsync($"{ex.Message}");
        }
    }
}
