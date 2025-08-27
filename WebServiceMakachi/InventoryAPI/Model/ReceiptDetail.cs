namespace InventoryAPI.Model
{
    public class ReceiptDetail
    {
        public string Barcodenumber { get; set; } = "";
        public int Quantity { get; set; }
        public decimal PriceSold { get; set; } = 0.00m;
        public int Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public string ProductName { get; set; } = "";
    }
}
