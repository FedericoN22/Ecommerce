using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_commerceApi.Domain.Entities.cart;
using E_commerceApi.Infrastructure.identity;

public class cartConfiguration : IEntityTypeConfiguration<cartETT>
{
    public void Configure(EntityTypeBuilder<cartETT> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.HasOne<ApplicationUsers>()
            .WithOne()
            .HasForeignKey<cartETT>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.UserId)
            .IsUnique();
    }
}