using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_commerceApi.Domain.Entities.order;

public class orderConfiguration : IEntityTypeConfiguration<OrderETT>
{
    public void Configure(EntityTypeBuilder<OrderETT> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.HasIndex(o => o.UserId);

        builder.Property(o => o.OrderDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(OrderETT.OrderStatus.Pending);
    }
}