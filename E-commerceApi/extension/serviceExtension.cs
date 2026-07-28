using E_commerceApi.Application.Interfaces;
using E_commerceApi.Application.Services;
using E_commerceApi.Infrastructure.Services;
namespace E_commerceApi.extension;

public static class ServiceExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

    }
}