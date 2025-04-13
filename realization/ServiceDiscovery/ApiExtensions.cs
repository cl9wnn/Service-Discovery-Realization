using API.Features.Services.AddServiceArea;
using API.Features.Services.DeleteServiceArea;
using API.Features.Services.GetAllServicesArea;
using API.Features.Services.GetServiceArea;
using API.Features.Services.UpdateServiceArea;
using API.Middlewares;

namespace API;

public static class ApiExtensions
{
    public static RouteGroupBuilder MapServicesApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllServices.Handle);
        group.MapGet("/{id:guid}", GetService.Handle);
        group.MapPost("/", AddService.Handle);
        group.MapPut("/{id:guid}", UpdateService.Handle);
        group.MapDelete("/{id:guid}", DeleteService.Handle);

        return group;
    }
    
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly); 

        return services;
    }
    
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}