namespace ApiGateway;

public class RoundRobinBalancer : IBalancer
{
    private int _currentIndex = -1;
    private readonly object _lock = new();

    public ServiceInfo? GetNextService(List<ServiceInfo> services)
    {
        if (services.Count == 0)
        {
            return null;
        }

        lock (_lock)
        {
            _currentIndex = (_currentIndex + 1) % services.Count;
            return services[_currentIndex];
        }
    }
}