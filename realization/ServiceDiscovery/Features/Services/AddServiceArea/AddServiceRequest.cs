namespace API.Features.Services.AddServiceArea;

public class AddServiceRequest
{
    public Guid Id { get; set; }

    public string Area { get; set; }

    public string Host { get; set; }

    public int Port { get; set; }
}
