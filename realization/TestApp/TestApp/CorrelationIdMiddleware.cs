namespace TestApp;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(AppConstants.CorrelationIdHeader, out var requestId) &&
            !string.IsNullOrWhiteSpace(requestId.ToString()))
        {
            var correlationId = requestId.ToString();
            context.Items.Add(AppConstants.CorrelationIdHeader, correlationId);

            logger.LogInformation($"Принят запрос {correlationId}");
            await next(context);
        }
        else
        {
            logger.LogWarning("Отсутствует traceId. Ответ: 404");
        
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("TraceId не найден");
        }
    }
}
