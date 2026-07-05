using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace E_commerceApi.extension;

public static class JWTExtension
{
    public static void AddJwtServices(this IServiceCollection services, IConfiguration Configuration)
    {
        services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = Configuration["Jwt:Issuer"],

                ValidAudience = Configuration["Jwt:Audience"],

                IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!))
            };
        });
    }
}





