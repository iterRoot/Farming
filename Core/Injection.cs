using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FarmingApi.Core;

public static class DependencyInjection
{
    public static void AddMiddleWare(this IApplicationBuilder app)
    {
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.AddError();
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        if (env != "Development") return;
        app.UseOpenApi();

        // NSwag v14+ method name
        app.UseSwaggerUi();
    }

    public static void AddInjection(this IServiceCollection service)
    {
        _assemblyInjection(service, "Service");
        _assemblyInjection(service, "SingletonService");
        _assemblyInjection(service, "Repository");

        // Register IMapper using a factory so we can obtain ILoggerFactory from the service provider
        // (AutoMapper v15 requires providing ILoggerFactory to MapperConfiguration constructor).
        service.AddSingleton<IMapper>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            // Configure maps by scanning the current assembly for Profile classes
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            }, loggerFactory);

            // Create mapper with service constructor so resolution of resolvers, type converters, etc. works.
            // NOTE: pass sp.GetService (Func<Type, object>) — not sp directly.
            return config.CreateMapper(sp.GetService);
        });

        service.AddMyCors();
        // service.AddMyAuthentication();
        service.AddMySwagger();
        service.AddControllers();
    }

    private static void _assemblyInjection(IServiceCollection service, string subFix)
    {
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(a => a.Name.EndsWith(subFix) && a is { IsAbstract: false, IsInterface: false })
            .Select(a => new { assignedType = a, serviceTypes = a.GetInterfaces().ToList() })
            .ToList()
            .ForEach(typesToRegister =>
            {
                if (subFix.Contains("Singleton"))
                {
                    typesToRegister.serviceTypes.ForEach(typeToRegister =>
                        service.AddSingleton(typeToRegister, typesToRegister.assignedType));
                }
                else
                {
                    typesToRegister.serviceTypes.ForEach(typeToRegister =>
                        service.AddScoped(typeToRegister, typesToRegister.assignedType));
                }
            });
    }
}
