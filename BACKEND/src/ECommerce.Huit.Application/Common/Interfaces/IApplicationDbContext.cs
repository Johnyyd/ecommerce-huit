using System.Data.Common;
using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Huit.Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();

    Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
    Task<int> ExecuteSqlRawAsync(string sql, System.Data.CommandBehavior behavior, params object[] parameters);

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
}
