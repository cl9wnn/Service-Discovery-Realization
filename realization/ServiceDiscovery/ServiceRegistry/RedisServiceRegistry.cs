using System.Text.Json;
using API.Models;
using StackExchange.Redis;

namespace API.ServiceRegistry;

public class RedisServiceRegistry(IConnectionMultiplexer redis) : IServiceRegistry
{
    private readonly IDatabase _db = redis.GetDatabase();
    private const string KeyPrefix = "service_registry:";

    public async Task RegisterAsync(ServiceInfo service)
    {
        var key = $"{KeyPrefix}{service.Id}";
        service.IsHealthy = true;

        var oldJson = await _db.StringGetAsync(key);
        if (!oldJson.IsNullOrEmpty)
        {
            var oldService = JsonSerializer.Deserialize<ServiceInfo>(oldJson!);
            if (oldService != null && oldService.Area != service.Area)
            {
                await _db.SetRemoveAsync($"area:{oldService.Area}", service.Id.ToString());
            }
        }

        var newJson = JsonSerializer.Serialize(service);
        await _db.StringSetAsync(key, newJson);
        await _db.SetAddAsync("service_registry:all_ids", service.Id.ToString());
        await _db.SetAddAsync($"area:{service.Area}", service.Id.ToString());
    }

    public async Task<ICollection<ServiceInfo>?> TryGetByAreaAsync(string area)
    {
        var ids = await _db.SetMembersAsync($"area:{area}");
        if (ids.Length == 0)
        {
            return null;
        }

        var services = new List<ServiceInfo>();
        foreach (var id in ids)
        {
            var json = await _db.StringGetAsync($"{KeyPrefix}{id}");
            if (!json.IsNullOrEmpty)
            {
                var service = JsonSerializer.Deserialize<ServiceInfo>(json);
                if (service != null)
                {
                    services.Add(service);
                }
            }
        }

        return services;
    }

    public async Task<ICollection<ServiceInfo>> GetAllAsync()
    {
        var ids = await _db.SetMembersAsync("service_registry:all_ids");

        var services = new List<ServiceInfo>();
        foreach (var id in ids)
        {
            var json = await _db.StringGetAsync($"{KeyPrefix}{id}");
            if (!json.IsNullOrEmpty)
            {
                var service = JsonSerializer.Deserialize<ServiceInfo>(json);
                if (service != null)
                    services.Add(service);
            }
        }

        return services;
    }

    public async Task<bool> UnregisterAsync(Guid id)
    {
        var key = $"{KeyPrefix}{id}";
        var json = await _db.StringGetAsync(key);
        if (json.IsNullOrEmpty)
        {
            return false;
        }

        var service = JsonSerializer.Deserialize<ServiceInfo>(json!);
        if (service == null)
        {
            return false;
        }

        service.IsHealthy = false;
        var updatedJson = JsonSerializer.Serialize(service);
        await _db.StringSetAsync(key, updatedJson);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var key = $"{KeyPrefix}{id}";
        var json = await _db.StringGetAsync(key);
        if (json.IsNullOrEmpty)
        {
            return false;
        }

        var service = JsonSerializer.Deserialize<ServiceInfo>(json!);
        if (service == null)
        {
            return false;
        }

        await _db.KeyDeleteAsync(key);
        await _db.SetRemoveAsync("service_registry:all_ids", id.ToString());
        await _db.SetRemoveAsync($"area:{service.Area}", id.ToString());
        return true;
    }

    public async Task<bool> UpdateAsync(ServiceInfo service)
    {
        var key = $"{KeyPrefix}{service.Id}";
        var oldJson = await _db.StringGetAsync(key);
        if (oldJson.IsNullOrEmpty)
        {
            return false;
        }

        var oldService = JsonSerializer.Deserialize<ServiceInfo>(oldJson!);
        if (oldService == null)
        {
            return false;
        }

        if (oldService.Area != service.Area)
        {
            await _db.SetRemoveAsync($"area:{oldService.Area}", service.Id.ToString());
            await _db.SetAddAsync($"area:{service.Area}", service.Id.ToString());
        }

        var newJson = JsonSerializer.Serialize(service);
        await _db.StringSetAsync(key, newJson);
        return true;
    }

    public async Task<ServiceInfo?> GetByIdAsync(Guid id)
    {
        var json = await _db.StringGetAsync($"{KeyPrefix}{id}");
        return json.IsNullOrEmpty ? null : JsonSerializer.Deserialize<ServiceInfo>(json!);
    }
}
