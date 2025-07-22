using Microsoft.AspNetCore.Identity;

namespace InventoryAPI.Model;

public class Stock
{
    public int? Id { get; set; }
    public int QuantityInStock { get; set; }
    public decimal CostUnitPrice { get; set; }
    public decimal SellingUnitPrice { get; set; }
    public decimal? Discount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public string StockNumber { get; set; }
    public decimal? FinalPrice { get; set; }
}
