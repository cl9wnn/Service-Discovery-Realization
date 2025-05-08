using API.Models;

namespace API.ServiceRegistry;

public interface IServiceRegistry
{
    Task RegisterAsync(ServiceInfo service);

    Task<ICollection<ServiceInfo>?> TryGetByAreaAsync(string area);

    Task<ICollection<ServiceInfo>> GetAllAsync();

    Task<bool> UnregisterAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);

    Task<bool> UpdateAsync(ServiceInfo service);

    Task<ServiceInfo?> GetByIdAsync(Guid id);
}
