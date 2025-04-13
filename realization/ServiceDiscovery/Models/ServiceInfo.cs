namespace API.Models;

public class ServiceInfo
{
    public Guid Id { get; set; }

    public string Area { get; set; }

    public int Port { get; set; }

    public DateTime RegisteredAt { get; set; }
}
