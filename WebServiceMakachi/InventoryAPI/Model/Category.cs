namespace InventoryAPI.Model;

public class Category
{
    //A t-shirt SKU might look like: TSH-BLK-MED-001 
    //    (TSH = T-shirt, BLK = Black, MED = Medium, 001 = Unique number)
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
