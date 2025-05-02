namespace API.Models;

public class ServiceInfo
{
    public Guid Id { get; set; }

    public string Area { get; set; }

    public string Host { get; set; }

    public int Port { get; set; }

    public DateTime RegisteredAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
