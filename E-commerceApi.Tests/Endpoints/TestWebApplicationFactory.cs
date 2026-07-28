using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Infrastructure.identity;
using E_commerceApi.Application.Interfaces;
using E_commerceApi.Middleware;
using E_commerceApi.Application.DTOs.Payment;
using E_commerceApi.extension;
using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Moq;

namespace E_commerceApi.Tests.Endpoints;

public class TestWebApplicationFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WebApplication _app;
    private readonly IConfiguration _configuration;

    public HttpClient Client { get; }
    public IServiceProvider Services => _app.Services;

    public TestWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(Program).Assembly.FullName,
        });

        builder.WebHost.UseTestServer();

        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "UnaClaveSuperSecretaDeMasDe32Caracteres",
            ["Jwt:Issuer"] = "ECommerceApi",
            ["Jwt:Audience"] = "ECommerceClient",
            ["Jwt:DurationInMinutes"] = "30",
            ["Stripe:SuccessUrl"] = "https://example.com/success?sessionId={CHECKOUT_SESSION_ID}",
            ["Stripe:CancelUrl"] = "https://example.com/cancel"
        });

        _configuration = builder.Configuration;
        ConfigureServices(builder.Services);

        _app = builder.Build();

        ConfigurePipeline(_app);

        _app.Start();
        Client = _app.GetTestServer().CreateClient();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(_connection));

        services.AddIdentity<ApplicationUsers, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "ECommerceApi",
                ValidAudience = "ECommerceClient",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("UnaClaveSuperSecretaDeMasDe32Caracteres"))
            };
        });

        services.AddAuthorization();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });

        services.AddRouting();

        services.AddServices();

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddFluentValidationAutoValidation();

        services.AddProblemDetails();
        services.AddScoped<ExceptionMiddleware>();

        services.AddScoped<IPaymentGateway>(_ =>
        {
            var mock = new Mock<IPaymentGateway>();
            mock.Setup(g => g.CreateCheckoutSessionAsync(
                It.IsAny<IEnumerable<CheckoutLineItem>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(("test_session_id", "https://checkout.stripe.com/test"));
            return mock.Object;
        });
    }

    private void ConfigurePipeline(WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();

        app.Use(async (context, next) =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null)
            {
                // Check if token is present
                var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader["Bearer ".Length..].Trim()
                    : null;
                if (token != null && token.Length > 20)
                {
                    // Token appears valid - let's proceed
                }
            }
            await next();
        });

        app.UseRouting();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthEndpoints();
        app.MapAdminEndpoints();
        app.MapPublicCatalogEndpoints();
        app.MapCartEndpoints();
        app.MapOrderEndpoints();
    }

    public void Dispose()
    {
        _app?.StopAsync().GetAwaiter().GetResult();
        _app?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _connection?.Dispose();
    }
}
