using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace InventoryAPI.Context;
public class ApplicationDbContext : IdentityDbContext
{
      public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
      {
      }
}
