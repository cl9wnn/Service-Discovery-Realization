namespace API.Models;

public class ServiceInfo
{
    public Guid Id { get; set; }

    public string Endpoint { get; set; }

    public DateTime RegisteredAt { get; set; }
}
