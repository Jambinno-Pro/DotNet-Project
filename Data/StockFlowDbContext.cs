using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Models;

namespace StockFlow.Api.Data;

public class StockFlowDbContext : IdentityDbContext<ApplicationUser>
{
    public StockFlowDbContext(DbContextOptions<StockFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<AppSetting> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.HasIndex(p => p.SKU)
                .HasDatabaseName("IX_Products_SKU")
                .IsUnique();

            entity.HasOne(p => p.CategoryNavigation)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.SupplierNavigation)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.WarehouseNavigation)
                .WithMany(w => w.Products)
                .HasForeignKey(p => p.WarehouseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name)
                .HasDatabaseName("IX_Categories_Name")
                .IsUnique();
        });

        // Supplier
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(s => s.Name)
                .HasDatabaseName("IX_Suppliers_Name")
                .IsUnique();
        });

        // Warehouse
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasIndex(w => w.Name)
                .HasDatabaseName("IX_Warehouses_Name")
                .IsUnique();

            entity.HasIndex(w => w.Code)
                .HasDatabaseName("IX_Warehouses_Code")
                .IsUnique();
        });

        // Stock Transaction
        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Warehouse)
                .WithMany()
                .HasForeignKey(t => t.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(t => t.ProductId)
                .HasDatabaseName("IX_StockTransactions_ProductId");

            entity.HasIndex(t => t.WarehouseId)
                .HasDatabaseName("IX_StockTransactions_WarehouseId");

            entity.HasIndex(t => t.CreatedAt)
                .HasDatabaseName("IX_StockTransactions_CreatedAt");
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.OrderNumber)
                .HasDatabaseName("IX_Orders_OrderNumber")
                .IsUnique();

            entity.Property(o => o.OrderNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(o => o.CustomerName)
                .HasMaxLength(200);

            entity.Property(o => o.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(o => o.Notes)
                .HasMaxLength(500);

            // Customer relationship
            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Order Item
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(item => item.LineTotal)
                .HasPrecision(18, 2);

            entity.HasIndex(item => item.OrderId)
                .HasDatabaseName("IX_OrderItems_OrderId");

            entity.HasIndex(item => item.ProductId)
                .HasDatabaseName("IX_OrderItems_ProductId");
        });
    }
}