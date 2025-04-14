using API.ServiceRegistry;
using AutoMapper;

namespace API.Features.Services.GetAllServicesArea;

public class GetAllServices()
{
    public static async Task<IResult> Handle(string area, HttpContext context, IServiceRegistry registry, IMapper mapper)
    {
        if (string.IsNullOrEmpty(area))
        {
            return Results.BadRequest("Не указана область сервиса");
        }

        var services = await registry.TryGetByAreaAsync(area);

        if (services == null || services.Count == 0)
        {
            return Results.NotFound("Сервисы не найдены");
        }

        var response = mapper.Map<GetAllServicesResponse>(services);
        return Results.Ok(response);
    }
}
