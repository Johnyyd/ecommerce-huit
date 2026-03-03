using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Huit.Infrastructure.Data.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(i => new { i.WarehouseId, i.VariantId });

        builder.Property(i => i.QuantityOnHand)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.QuantityReserved)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.ReorderPoint)
            .IsRequired()
            .HasDefaultValue(10);

        // Relationships
        builder.HasOne(i => i.Warehouse)
            .WithMany(w => w.Inventories)
            .HasForeignKey(i => i.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Variant)
            .WithMany(v => v.Inventories)
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes (composite PK đã là index, thêm index cho variant)
        builder.HasIndex(i => i.VariantId);
        builder.HasIndex(i => new { i.QuantityOnHand, i.ReorderPoint })
            .HasFilter("[QuantityOnHand] <= [ReorderPoint]")
            .HasDatabaseName("IX_Inventories_LowStock");
    }
}
