namespace API.HealthChecker;

public interface IHealthChecker
{
    Task CheckAllServicesAsync();
}
