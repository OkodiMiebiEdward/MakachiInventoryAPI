namespace InventoryAPI.Model.DTO;

public class ProductDTO
{
    public int? Id { get; set; }
    public string ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public int CategoryId { get; set; }
    public string SKU { get; set; }
    public List<VariantDTO> Variants { get; set; } = new List<VariantDTO>();
}
