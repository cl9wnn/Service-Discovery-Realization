namespace ApiGateway;

public class ServiceInfo
{
    public Guid Id { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public bool IsHealthy { get; set; }
}