using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Huit.Domain.Entities;

namespace ECommerce.Huit.Application.Common.Interfaces
{
    public interface IApplicationDbContext : IDisposable
    {
        DbSet<T> Set<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        int SaveChanges();

        DbSet<User> Users { get; }
        DbSet<Product> Products { get; }
        DbSet<ProductVariant> ProductVariants { get; }
        DbSet<Category> Categories { get; }
        DbSet<Brand> Brands { get; }
        DbSet<Cart> Carts { get; }
        DbSet<CartItem> CartItems { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderItem> OrderItems { get; }
        DbSet<Inventory> Inventories { get; }
        DbSet<Warehouse> Warehouses { get; }
        DbSet<Voucher> Vouchers { get; }
        DbSet<Address> Addresses { get; }
        DbSet<StockMovement> StockMovements { get; }
        DbSet<VoucherUsage> VoucherUsages { get; }
        DbSet<OrderStatusHistory> OrderStatusHistories { get; }
        DbSet<OrderItemSerial> OrderItemSerials { get; }
    }
}
