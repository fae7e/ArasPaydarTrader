using ArasTrader.Web.Api.Endpoints;
using System.Reflection;

namespace ArasTrader.Web.Api.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var endpointTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(EndpointBase)) && !t.IsAbstract);

        foreach (var type in endpointTypes)
            services.AddTransient(typeof(EndpointBase), type);

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<EndpointBase>>();

        foreach (var endpoint in endpoints)
            endpoint.MapEndpoint(app);

        return app;
    }
}
