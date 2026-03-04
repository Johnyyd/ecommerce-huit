using Microsoft.EntityFrameworkCore;
using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Huit.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Code)
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(o => o.OrderType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (OrderType)Enum.Parse(typeof(OrderType), v));

        builder.Property(o => o.Subtotal)
            .HasColumnType("DECIMAL(15,2)")
            .IsRequired();

        builder.Property(o => o.Discount)
            .HasColumnType("DECIMAL(15,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.ShippingFee)
            .HasColumnType("DECIMAL(15,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.Total)
            .HasColumnType("DECIMAL(15,2)")
            .IsRequired();

        builder.Property(o => o.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.PaymentStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v));

        builder.Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v));

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(o => o.Note)
            .HasColumnType("NVARCHAR(MAX)");

        // Indexes
        builder.HasIndex(o => o.Code).IsUnique();
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PaymentStatus);

        // Relationships
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
