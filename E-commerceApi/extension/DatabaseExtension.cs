using Microsoft.EntityFrameworkCore;
using E_commerceApi.Infrastructure.Data;

namespace E_commerceApi.extension
{
    public static class DatabaseExtension
    {
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });
        }
    }
}
