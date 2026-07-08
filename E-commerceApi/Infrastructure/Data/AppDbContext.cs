using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using E_commerceApi.Infrastructure.identity;
using E_commerceApi.Domain.Entities.intefaces;

namespace E_commerceApi.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUsers>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

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