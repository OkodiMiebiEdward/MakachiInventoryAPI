using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Model;

public class AssignRoleVM
{
    public string UserName { get; set; }
    public string Role { get; set; }
}
