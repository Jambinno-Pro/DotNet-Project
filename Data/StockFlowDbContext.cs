using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Models;

namespace StockFlow.Api.Data;

public class StockFlowDbContext : DbContext
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
    }
}