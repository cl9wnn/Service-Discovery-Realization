using System.Collections.Concurrent;
using API.Models;

namespace API.ServiceRegistry;

public class InMemoryServiceRegistry : IServiceRegistry
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, ServiceInfo>> _services = new ();

    public async Task RegisterAsync(ServiceInfo service)
    {
        var areaServices = _services
            .GetOrAdd(service.Area, _ => new ConcurrentDictionary<Guid, ServiceInfo>());

        areaServices[service.Id] = service;
    }

    public async Task<ICollection<ServiceInfo>?> TryGetByAreaAsync(string area)
    {
        return !_services.TryGetValue(area, out var areaServices)
            ? null
            : areaServices.Values;
    }

    public async Task<ICollection<ServiceInfo>> GetAllAsync()
    {
        return _services.Values
            .SelectMany(area => area.Values)
            .ToList();
    }

    public async Task<bool> UnregisterAsync(Guid id)
    {
        foreach (var area in _services.Values)
        {
            if (!area.TryRemove(id, out _))
            {
                continue;
            }

            if (!area.IsEmpty)
            {
                return true;
            }

            var areaName = _services.FirstOrDefault(x => x.Value == area).Key;
            _services.TryRemove(areaName, out _);

            return true;
        }

        return false;
    }

    public async Task<bool> UpdateAsync(ServiceInfo service)
    {
        if (!_services.TryGetValue(service.Area, out var areaServices))
        {
            return false;
        }

        return areaServices.TryGetValue(service.Id, out var oldService) &&
               areaServices.TryUpdate(service.Id, service, oldService);
    }

    public async Task<ServiceInfo?> GetByIdAsync(Guid id)
    {
        foreach (var area in _services.Values)
        {
            if (area.TryGetValue(id, out var service))
            {
                return service;
            }
        }

        return null;
    }
}
