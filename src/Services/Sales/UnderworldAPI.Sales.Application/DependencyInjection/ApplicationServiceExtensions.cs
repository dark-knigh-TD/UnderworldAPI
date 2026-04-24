using System;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UnderworldAPI.Sales.Application.Behaviors;

namespace UnderworldAPI.Sales.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection ApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceExtensions).Assembly;

        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        //TODO revisar este codigo no complia
        //services.AddAutoMapper(assembly);

        return services;

    }
}
