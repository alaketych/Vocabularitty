using Microsoft.OpenApi.Models;

namespace Vocabularity.Api.Configuration;

public static class ApiDocumentation
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<PaginationDocumentation>();
            options.OperationFilter<OperationResponseDocumentation>();
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Vocabularity API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste the access_token from register or login."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = []
            });
        });

        services.AddSwaggerGenNewtonsoftSupport();
        return services;
    }
}

