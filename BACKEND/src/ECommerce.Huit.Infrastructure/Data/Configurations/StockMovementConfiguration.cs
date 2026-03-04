using Microsoft.EntityFrameworkCore;
using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Huit.Infrastructure.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Quantity)
            .IsRequired();

        builder.Property(sm => sm.MovementType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sm => sm.ReferenceType)
            .HasMaxLength(50);

        builder.Property(sm => sm.Note)
            .HasColumnType("NVARCHAR(MAX)");

        // Relationships
        builder.HasOne(sm => sm.Warehouse)
            .WithMany(w => w.StockMovements)
            .HasForeignKey(sm => sm.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.Variant)
            .WithMany()  // No navigation property on ProductVariant
            .HasForeignKey(sm => sm.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.Supplier)
            .WithMany(s => s.StockMovements)
            .HasForeignKey(sm => sm.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
