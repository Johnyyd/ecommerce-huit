using System.Data.Common;
using System.Text;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ECommerce.Huit.Infrastructure.Data.Configurations;

namespace ECommerce.Huit.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Users & Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Catalog
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    // Warehouse & Inventory
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<ProductSerial> ProductSerials => Set<ProductSerial>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Cart
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemSerial> OrderItemSerials => Set<OrderItemSerial>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Marketing
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();

    // Support
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<Return> Returns => Set<Return>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Ngan loi Multiple Cascade Paths cho bang Return
        modelBuilder.Entity<Return>().HasOne(r => r.Order).WithMany().HasForeignKey(r => r.OrderId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction);
        modelBuilder.Entity<Return>().HasOne(r => r.OrderItem).WithMany().HasForeignKey(r => r.OrderItemId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.NoAction);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        // ... other configurations

        // Seed permissions (from init.sql)
        modelBuilder.Entity<ECommerce.Huit.Domain.Entities.Permission>().HasData(
            new ECommerce.Huit.Domain.Entities.Permission { Id = 1, Code = "products.read", Name = "Xem sản phẩm", Module = "PRODUCT" },
            new ECommerce.Huit.Domain.Entities.Permission { Id = 2, Code = "products.create", Name = "Tạo sản phẩm", Module = "PRODUCT" }
            // ... add more
        );

        // Apply snake_case naming convention for columns and tables to match existing database schema
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Map columns to snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            // Map tables to existing snake_case table names
            var defaultTableName = entity.GetTableName(); // usually the DbSet property name or entity name
            if (defaultTableName != null)
            {
                // Handle special cases where table name doesn't follow simple snake_case conversion of DbSet name
                var snakeCaseTable = ToSnakeCase(defaultTableName);

                // Special case: OrderStatusHistories -> order_status_history (singular)
                if (defaultTableName == "OrderStatusHistories")
                {
                    entity.SetTableName("order_status_history");
                }
                else
                {
                    entity.SetTableName(snakeCaseTable);
                }
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]))
            {
                if (i > 0)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(name[i]));
            }
            else
            {
                sb.Append(name[i]);
            }
        }
        return sb.ToString();
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
    {
        return base.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    public Task<int> ExecuteSqlRawAsync(string sql, System.Data.CommandBehavior behavior, params object[] parameters)
    {
        return base.Database.ExecuteSqlRawAsync(sql, behavior, parameters);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
            }

            ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
