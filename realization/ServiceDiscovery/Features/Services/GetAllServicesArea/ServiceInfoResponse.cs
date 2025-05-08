namespace API.Features.Services.GetAllServicesArea;

public class ServiceInfoResponse
{
    public Guid Id { get; set; }
    public int Port { get; set; }
    public string Host { get; set; }
    public bool IsHealthy { get; set; }
}
