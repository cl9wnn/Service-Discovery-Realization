namespace ApiGateway;

public interface IBalancer
{
    ServiceInfo? GetNextService(List<ServiceInfo> services);
}