using System.Collections.Concurrent;
using API.Models;

namespace API.ServiceRegistry;

public class InMemoryServiceRegistry : IServiceRegistry
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, ServiceInfo>> _services = new ();

    public Task RegisterAsync(ServiceInfo service)
    {
        var areaServices = _services.GetOrAdd(service.Area, _ => new ConcurrentDictionary<Guid, ServiceInfo>());

        service.IsHealthy = true;
        areaServices.AddOrUpdate(service.Id, service, (_, existing) =>
        {
            existing.Host = service.Host;
            existing.Port = service.Port;
            existing.IsHealthy = true;
            return existing;
        });

        return Task.CompletedTask;
    }


    public Task<ICollection<ServiceInfo>?> TryGetByAreaAsync(string area)
    {
        if (!_services.TryGetValue(area, out var areaServices))
        {
            return Task.FromResult<ICollection<ServiceInfo>?>(null);
        }

        return Task.FromResult<ICollection<ServiceInfo>>(areaServices.Values.ToList());
    }

    public Task<ICollection<ServiceInfo>> GetAllAsync()
    {
        var all = _services.Values
            .SelectMany(dict => dict.Values)
            .ToList();

        return Task.FromResult<ICollection<ServiceInfo>>(all);
    }

    public Task<bool> UnregisterAsync(Guid id)
    {
        foreach (var area in _services.Values)
        {
            if (area.TryGetValue(id, out var service))
            {
                service.IsHealthy = false;
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    public Task<bool> UpdateAsync(ServiceInfo service)
    {
        if (!_services.TryGetValue(service.Area, out var areaServices))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(areaServices.TryUpdate(service.Id, service, areaServices[service.Id]));
    }

    public Task<ServiceInfo?> GetByIdAsync(Guid id)
    {
        foreach (var area in _services.Values)
        {
            if (area.TryGetValue(id, out var service))
            {
                return Task.FromResult<ServiceInfo?>(service);
            }
        }

        return Task.FromResult<ServiceInfo?>(null);
    }
}
