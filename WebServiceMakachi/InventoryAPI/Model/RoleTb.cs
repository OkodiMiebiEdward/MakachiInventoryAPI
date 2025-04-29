using Microsoft.AspNetCore.Identity;

namespace InventoryAPI.Model;

public class RoleTb: IdentityRole
{
    public string? RoleDescription { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public int AccessLevel { get; set; }
}
