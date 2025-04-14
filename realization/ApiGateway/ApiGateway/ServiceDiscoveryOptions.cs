namespace ApiGateway;

public class ServiceDiscoveryOptions
{
    // TODO: Почему то не грузит из конфига
    private string Scheme { get; set; } = "http";
    
    public string Host { get; set; }
    
    public int Port { get; set; }
    
    public string Path { get; set; }
    
    public string GetDiscoveryUrl(string area)
    {
        var uriBuilder = new UriBuilder
        {
            Scheme = Scheme,
            Host = Host,
            Port = Port,
            Path = $"{Path.Trim('/')}/{area.Trim('/')}"
        };
        
        return uriBuilder.Uri.ToString();
    }
}