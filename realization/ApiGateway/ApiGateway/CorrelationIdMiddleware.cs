namespace ApiGateway;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;
        if (context.Request.Headers.TryGetValue(AppConstants.CorrelationIdHeader, out var requestId) &&
            !string.IsNullOrWhiteSpace(requestId.ToString()))
        {
            correlationId = requestId.ToString(); 
        }
        else
        {
            correlationId = Guid.NewGuid().ToString();
        }
        
        context.Items.Add(AppConstants.CorrelationIdHeader, correlationId);

        await next(context);
    }
}