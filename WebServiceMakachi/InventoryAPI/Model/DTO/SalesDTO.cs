namespace InventoryAPI.Model.DTO
{
    public class SalesDTO
    {
        public int? Id { get; set; }
        public decimal Discount { get; set; }
        public decimal PriceSold { get; set; }
        public int Quantity { get; set; }
        public int? StockId { get; set; }
        public string Barcodenumber { get; set; }
    }
}
