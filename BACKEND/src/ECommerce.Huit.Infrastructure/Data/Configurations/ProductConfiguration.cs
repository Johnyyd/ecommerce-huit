using Microsoft.EntityFrameworkCore;
using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Huit.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (ProductStatus)Enum.Parse(typeof(ProductStatus), v));

        builder.Property(p => p.Specifications)
            .HasColumnType("NVARCHAR(MAX)");

        // Indexes
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.BrandId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsFeatured);

        // Relationships
        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
