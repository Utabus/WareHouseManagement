using System;

namespace WareHouseManagement.Models
{
    public class StockReport
    {
        public string ProductName { get; set; }
        public string Series { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal Profit => (SellPrice - CostPrice) * Quantity; // Tính tự động
        public DateTime LastUpdated { get; set; }
    }
}
