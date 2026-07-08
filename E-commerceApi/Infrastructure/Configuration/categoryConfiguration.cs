using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using E_commerceApi.Domain.Entities.category;

public class CategoryConfiguration : IEntityTypeConfiguration<categoryETT>
{
    public void Configure(EntityTypeBuilder<categoryETT> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Name)
            .IsUnique();


        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedDateAud)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}