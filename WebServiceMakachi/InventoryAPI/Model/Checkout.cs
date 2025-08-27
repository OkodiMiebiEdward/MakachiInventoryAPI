using SkiaSharp;

namespace InventoryAPI.Model
{
    public class Checkout
    {
        public int Id { get; set; }
        public string LoggedInUser { get; set; } = "";
        public List<SubData> SubData { get; set; } = new();
    }

    public class SubData
    {
        public string Barcodenumber { get; set; } = "";
        public int Quantity { get; set; }
        public decimal PriceSold { get; set; } = 0.00m;
        public int Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public string ProductName { get; set; } = "";
    }
}
