using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace InventoryAPI.Model;

public class User : IdentityUser
{
    public DateTime? CreationDate { get; set; } = DateTime.Now;
    
    public string? Name { get; set; }

    public string? StaffCode { get; set; }
    
    public bool FirstLogIn { get; set; }
    
    public bool PwdExpiry { get; set; }
    
    public DateTime? PwdExpiryDate { get; set; }
    
    public bool Suspended { get; set; }

    //[MaxLength(1)]
    public string? AccessLevel { get; set; }

    public string? LoginToken { get; set; }
    
}
