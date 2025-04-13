using API.Models;

namespace API.ServiceRegistry;

public interface IServiceRegistry
{
    void Register(ServiceInfo service);
    
    bool TryGet(Guid id, out ServiceInfo service);
    
    ICollection<ServiceInfo> GetAll();
    
    bool Unregister(Guid id);
    
    bool Update(ServiceInfo service);
}