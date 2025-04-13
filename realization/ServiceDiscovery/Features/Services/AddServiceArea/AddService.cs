using API.Models;
using API.ServiceRegistry;
using AutoMapper;

namespace API.Features.Services.AddServiceArea;

public class AddService
{
    public static async Task<IResult> Handle(HttpContext context, IServiceRegistry registry, IMapper mapper)
    {
        var request = await context.Request.ReadFromJsonAsync<AddServiceRequest>();

        if (request is null)
        {
            return Results.BadRequest();
        }

        var service = mapper.Map<ServiceInfo>(request);

        await registry.RegisterAsync(service);

        return Results.Ok(service);
    }
}
