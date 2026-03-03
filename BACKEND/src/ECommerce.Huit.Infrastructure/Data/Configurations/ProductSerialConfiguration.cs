using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Huit.Infrastructure.Data.Configurations;

public class ProductSerialConfiguration : IEntityTypeConfiguration<ProductSerial>
{
    public void Configure(EntityTypeBuilder<ProductSerial> builder)
    {
        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.SerialNumber)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(ps => ps.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (SerialStatus)Enum.Parse(typeof(SerialStatus), v));

        builder.Property(ps => ps.InboundDate)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(ps => ps.WarrantyExpireDate)
            .HasColumnType("DATE");

        // Indexes
        builder.HasIndex(ps => ps.SerialNumber).IsUnique();
        builder.HasIndex(ps => ps.VariantId);
        builder.HasIndex(ps => ps.WarehouseId);
        builder.HasIndex(ps => ps.Status);

        // Relationships
        builder.HasOne(ps => ps.Variant)
            .WithMany(v => v.Serials)
            .HasForeignKey(ps => ps.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Warehouse)
            .WithMany(w => w.Serials)
            .HasForeignKey(ps => ps.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
