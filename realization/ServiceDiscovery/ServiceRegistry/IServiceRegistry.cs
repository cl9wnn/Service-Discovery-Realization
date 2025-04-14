using API.Models;

namespace API.ServiceRegistry;

public interface IServiceRegistry
{
    // TODO: Подумать, что если регистрируется уже существующая реплика
    Task RegisterAsync(ServiceInfo service);

    // TODO: Подумать куда вставить Round-Robin
    Task<ServiceInfo?> TryGetByAreaAsync(string area);

    Task<ICollection<ServiceInfo>> GetAllAsync();

    Task<bool> UnregisterAsync(Guid id);

    Task<bool> UpdateAsync(ServiceInfo service);

    Task<ServiceInfo?> GetByIdAsync(Guid id);
}
