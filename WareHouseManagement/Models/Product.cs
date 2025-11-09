using System;

namespace WareHouseManagement.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Series { get; set; }
        public string ProductName { get; set; }
        public string Color { get; set; }
        public string Capacity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime ImportDate { get; set; }
        public bool IsSold { get; set; }

        // 🔹 Liên kết đến loại sản phẩm
        public int ProductTypeId { get; set; }      // FK -> ProductType
        public string ProductTypeName { get; set; } // Hiển thị khi join hoặc truy vấn
    }
}
