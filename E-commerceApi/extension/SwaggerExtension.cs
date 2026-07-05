using Microsoft.OpenApi.Models;


namespace E_commerceApi.extension;

public static class SwaggerExtension
{
    public static void AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "ECommerce API",

                    Version = "v1"
                });

            options.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,

                    Description = "Ingrese el JWT",

                    Name = "Authorization",

                    Type = SecuritySchemeType.Http,

                    Scheme = "Bearer",

                    BearerFormat = "JWT"
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                []
            }
                });
        });


    }

    public static void UseSwaggerServices(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
            options.RoutePrefix = string.Empty; // Swagger en la raíz: https://localhost:5001/
        });
    }
}



