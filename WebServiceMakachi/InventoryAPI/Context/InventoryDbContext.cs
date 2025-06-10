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

            // Configure Name property
            entity.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Configure Value property
            entity.Property(v => v.Value)
                .IsRequired()
                .HasMaxLength(200);

            // Define relationship with Product table (Many-to-One)
            entity.HasOne(v => v.Product)
                .WithMany(p => p.Variants) // Assuming Product has a List<Variant>
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // If a product is deleted, its variants should be removed too.

            // Add an index for faster lookup on Name and Value combinations
            entity.HasIndex(v => new { v.Name, v.Value });
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
                .HasForeignKey("CategoryId") // Explicit Foreign Key
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

        base.OnModelCreating(modelBuilder);
    }
}
