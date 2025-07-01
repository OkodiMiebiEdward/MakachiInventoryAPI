using InventoryAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Context;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Variant> Variants { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Sales> Sales { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            // Set primary key
            entity.HasKey(c => c.Id);

            // Configure Name property
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Configure Description property as optional with length
            entity.Property(c => c.Description)
                .HasMaxLength(500);

            // Default value for IsActive
            entity.Property(c => c.IsActive)
                .HasDefaultValue(true);

            // Add an index for faster lookup on Name
            entity.HasIndex(c => c.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Variant>(entity =>
        {
            // Set primary key
            entity.HasKey(v => v.Id);

            // Configure Size property
            entity.Property(v => v.Size)
                .IsRequired()
                .HasMaxLength(100);

            // Configure Color property
            entity.Property(v => v.Color)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)"); // Precision: 18, Scale: 2

            // Define relationship with Product table (Many-to-One)
            entity.HasOne(v => v.Product)
                .WithMany(p => p.Variants) // Assuming Product has a List<Variant>
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // If a product is deleted, its variants should be removed too.

            // Add an index for faster lookup on Name and Value combinations
            //entity.HasIndex(v => new { v.Name, v.Value });
        });

        modelBuilder.Entity<Product>(entity =>
        {
            // Set primary key
            entity.HasKey(p => p.Id);

            // Configure ProductName property
            entity.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(150); // Limit the length to avoid excessive storage

            // Configure ProductDescription as optional
            entity.Property(p => p.ProductDescription)
                .HasMaxLength(500); // Allows description but limits length

            // Configure SKU property
            entity.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            // Configure BarCodeNumber property
            entity.Property(p => p.BarCodeNumber)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            // Define relationship with Category (Many-to-One)
            entity.HasOne(p => p.Category)
                .WithMany() // Assuming Category does not have a Product list
                .HasForeignKey(p => p.CategoryId) // Explicit Foreign Key
                .OnDelete(DeleteBehavior.Restrict); // Prevent unintended deletions

            // Define relationship with Variant (One-to-Many)
            entity.HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a product removes all variants


            // Add an index for faster lookup on SKU and BarCodeNumber
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.HasIndex(p => p.BarCodeNumber).IsUnique();
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.QuantityInStock)
                  .IsRequired();

            entity.Property(e => e.CostUnitPrice)
             .HasColumnType("decimal(18,2)")
             .IsRequired();

            entity.Property(e => e.SellingUnitPrice)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired();

            entity.Property(e => e.Discount)
                  .HasColumnType("decimal(5,2)") // If it's a percentage e.g., 15.50%
                  .IsRequired(false);

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()"); // SQL Server default

            // Foreign key relationship with Category
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Foreign key relationship with Product
            entity.HasOne(e => e.Product)
                  .WithMany() // If Product has a List<Stock>, use .WithMany(p => p.Stocks)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sales>(entity =>
        {
            entity.HasKey(e => e.Id);

            // StockId is required
            entity.Property(s => s.StockId)
                .IsRequired();

            // Quantity is required
            entity.Property(s => s.Quantity)
                .IsRequired();

            // PriceSold is required, precision for money
            entity.Property(s => s.PriceSold)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // Discount is required, precision for money
            entity.Property(s => s.Discount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // Relationship: Sales -> Stock (many-to-one)
            entity.HasOne(s => s.Stock)
                .WithMany()
                .HasForeignKey(s => s.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        base.OnModelCreating(modelBuilder);
    }
}
