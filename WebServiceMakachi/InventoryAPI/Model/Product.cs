namespace InventoryAPI.Model;

public class Product
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public Category Category { get; set; }
    public string SKU { get; set; }
    public string BarCodeNumber { get; set; }
    public List<Variant> Variants { get; set; } = new List<Variant>();
}
