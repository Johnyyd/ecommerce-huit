using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Huit.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(oi => oi.Sku)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);

        builder.Property(oi => oi.UnitPrice)
            .HasColumnType("DECIMAL(15,2)")
            .IsRequired();

        builder.Property(oi => oi.TotalPrice)
            .HasColumnType("DECIMAL(15,2)")
            .IsRequired();

        // Relationships
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Variant)
            .WithMany(v => v.OrderItems)
            .HasForeignKey(oi => oi.VariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
