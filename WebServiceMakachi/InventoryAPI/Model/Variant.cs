namespace InventoryAPI.Model;

public class Variant
{
    public int Id { get; set; } // Primary Key
    public string Size { get; set; }
    public string Color { get; set; }
    public decimal Price { get; set; }
    public int ProductId { get; set; } // Foreign Key linking to Product
    public Product Product { get; set; }
}
