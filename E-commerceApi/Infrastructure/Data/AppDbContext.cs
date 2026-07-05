using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using E_commerceApi.Domain.Entities;

namespace E_commerceApi.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUsers>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Define your DbSets for your entities here
        // Example:
        // public DbSet<Product> Products { get; set; }
    }
}