namespace API.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(AppConstants.CorrelationIdHeader, out var requestId) &&
            !string.IsNullOrWhiteSpace(requestId.ToString()))
        {
            var correlationId = requestId.ToString();
            context.Items.Add(AppConstants.CorrelationIdHeader, correlationId);

            logger.LogInformation("Принят запрос {@correlationId}", correlationId);
        }
        else
        {
            logger.LogInformation("Отсутствует traceId. Выход");
        }

        await next(context);
    }
}
