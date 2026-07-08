using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_commerceApi.Domain.Entities.orderItem;
using E_commerceApi.Domain.Entities.order;
using E_commerceApi.Domain.Entities.product;

public class orderItemConfiguration : IEntityTypeConfiguration<orderItemETT>
{
    public void Configure(EntityTypeBuilder<orderItemETT> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.HasIndex(oi => oi.OrderId);

        builder.HasOne<OrderETT>()
            .WithMany()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<productETT>()
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // builder.HasIndex(oi => oi.ProductId);

        builder.Property(oi => oi.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(oi => oi.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
    }
}