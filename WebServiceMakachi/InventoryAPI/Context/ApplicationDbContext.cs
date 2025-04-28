using InventoryAPI.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace InventoryAPI.Context;
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
    {
    }

    public DbSet<User> User { get; set; }
    public DbSet<RoleTb> RoleTb { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
              .Property(u => u.Name)
              .IsRequired()
              .HasMaxLength(255);

        builder.Entity<User>()
              .Property(u => u.CreationDate)
              .HasColumnType("datetime");

        builder.Entity<User>()
              .Property(u => u.FirstLogIn)
              .HasDefaultValue(false);

        builder.Entity<User>()
             .Property(u => u.PwdExpiry)
             .HasDefaultValue(false);

        builder.Entity<User>()
            .Property(u => u.PwdExpiryDate)
            .HasColumnType("datetime");

        builder.Entity<User>()
           .Property(u => u.Suspended)
           .HasDefaultValue(false);

        builder.Entity<User>()
           .Property(u => u.AccessLevel)
           .HasMaxLength(1);

        base.OnModelCreating(builder);
    }

}
