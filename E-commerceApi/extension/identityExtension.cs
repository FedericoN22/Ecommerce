using E_commerceApi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using E_commerceApi.Infrastructure.identity;

namespace E_commerceApi.extension
{
    public static class IdentityExtension
    {
        public static void AddIdentityServices(this IServiceCollection services)
        {

            services
                .AddIdentity<ApplicationUsers, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
        }
    }
}
