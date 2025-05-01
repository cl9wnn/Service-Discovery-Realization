using System.Reflection;
using API.Features.Services.AddServiceArea;
using API.Features.Services.DeleteServiceArea;
using API.Features.Services.GetAllServicesArea;
using API.Middlewares;

namespace API;

public static class ApiExtensions
{
    public static RouteGroupBuilder MapServicesApi(this RouteGroupBuilder group)
    {
        group.MapGet("/{area}", GetAllServices.Handle);
        group.MapPost("/", AddService.Handle);
        group.MapDelete("/{id:guid}", DeleteService.Handle);

        return group;
    }

    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
