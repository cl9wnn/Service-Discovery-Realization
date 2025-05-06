using System.Reflection;
using API.Features.Services.AddServiceArea;
using API.Features.Services.DeleteServiceArea;
using API.Features.Services.GetAllServicesArea;
using API.Middlewares;
using Serilog;
using Serilog.Events;

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

    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var seqUrl = builder.Configuration["Seq:Url"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .WriteTo.Seq(seqUrl!)
            .Enrich.WithProperty("Application", "ServiceDiscovery")
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}
