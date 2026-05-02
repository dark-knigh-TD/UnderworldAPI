using System;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace UnderworldAPI.Sales.API.DependencyInjection;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();

        // API Versioning — permite manejar v1, v2 sin romper clientes existentes
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            // Versión en la URL: /api/v1/orders
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Suprimir el modelstate 400 automático — manejamos errores con Result<T>
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        // OpenAPI nativo .NET 10
        services.AddOpenApi();

        return services;
    }
}
