using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Domain.Entities;

namespace ECommerce.Huit.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
            // Set initializer to null for existing database
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public ApplicationDbContext(string connectionString) : base(connectionString)
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        // Users & Auth
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // Catalog
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        // Warehouse & Inventory
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<ProductSerial> ProductSerials { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        // Cart
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // Orders
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemSerial> OrderItemSerials { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // Marketing
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherUsage> VoucherUsages { get; set; }

        // Support
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Return> Returns { get; set; }

        // Audit
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EF6 uses singularized table names by default
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Configure snake_case naming convention (Manual mapping for EF6)
            // In EF6, the easiest way is to use EntityTypeConfiguration for each entity.
            // For brevity in this migration, let's keep default EF6 naming or specific overrides.

            // Example:
            modelBuilder.Entity<OrderStatusHistory>().ToTable("order_status_history");
            
            // Fix Multiple Cascade Paths for Return
            modelBuilder.Entity<Return>()
                .HasRequired(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Return>()
                .HasRequired(r => r.OrderItem)
                .WithMany()
                .HasForeignKey(r => r.OrderItemId)
                .WillCascadeOnDelete(false);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            UpdateTimestamps();
            return base.SaveChangesAsync();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;
                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
