using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Model.DTO;

public class StockDTO
{
    public int? Id { get; set; }
    [Required]
    public int QuantityInStock { get; set; }
    [Required]
    public decimal CostUnitPrice { get; set; }
    [Required]
    public decimal SellingUnitPrice { get; set; }
    public decimal? Discount { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public int CategoryId { get; set; }
    [Required]
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string StockNumber { get; set; } = "";
    public string BarCodeNumber { get; set; }
    public decimal? FinalPrice { get; set; }
}
