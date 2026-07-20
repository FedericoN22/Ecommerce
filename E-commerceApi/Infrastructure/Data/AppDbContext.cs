using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using E_commerceApi.Infrastructure.identity;
using E_commerceApi.Domain.Entities.intefaces;

using E_commerceApi.Domain.Entities.product;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.cart;
using E_commerceApi.Domain.Entities.cartItem;
using E_commerceApi.Domain.Entities.order;
using E_commerceApi.Domain.Entities.orderItem;

namespace E_commerceApi.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUsers>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<productETT> products { get; set; }
        public DbSet<categoryETT> categories { get; set; }
        public DbSet<cartETT> carts { get; set; }
        public DbSet<cartItemETT> cartItems { get; set; }
        public DbSet<OrderETT> orders { get; set; }
        public DbSet<orderItemETT> orderItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<IAuditableEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDateAud = DateTime.UtcNow;
                }

                entry.Entity.ModifiedDateAud = DateTime.UtcNow;

            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }
}