using System;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using UnderworldAPI.Purchases.Application.Behaviors;

namespace UnderworldAPI.Purchases.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceExtensions).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        //TODO revisar este codigo no compila
        //services.AddAutoMapper(assembly);
         services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        return services;
    }
}
