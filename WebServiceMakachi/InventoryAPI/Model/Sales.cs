namespace InventoryAPI.Model
{
    public class Sales
    {
        public int? Id { get; set; }
        public int StockId { get; set; }
        public Stock Stock { get; set; }
        public int Quantity { get; set; }
        public decimal PriceSold { get; set; }
        public decimal Discount { get; set; }
        public string Barcodenumber { get; set; }
    }
}
