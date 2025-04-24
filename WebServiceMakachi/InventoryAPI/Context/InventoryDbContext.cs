using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Context;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }
}
