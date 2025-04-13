using System.Collections.Concurrent;
using API.Models;

namespace API.ServiceRegistry;

public class InMemoryServiceRegistry: IServiceRegistry
{
    private readonly ConcurrentDictionary<Guid, ServiceInfo> _services = new();

    public void Register(ServiceInfo service)
    {
        _services[service.Id] = service;
    }

    public bool TryGet(Guid id, out ServiceInfo service)
    {
        return _services.TryGetValue(id, out service!);
    }

    public ICollection<ServiceInfo> GetAll()
    {
        return _services.Values;
    }

    public bool Unregister(Guid id)
    {
        return _services.TryRemove(id, out _);
    }

    public bool Update(ServiceInfo service)
    {
        return _services.TryUpdate(service.Id, service, _services[service.Id]);
    }
}