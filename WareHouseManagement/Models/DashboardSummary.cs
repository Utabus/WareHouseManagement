using System;

namespace WareHouseManagement.Models
{
    public class DashboardSummary
    {
        public int TotalProducts { get; set; }      // Tổng số mặt hàng
        public int TotalInStock { get; set; }       // Tổng hàng tồn
        public decimal TotalImportValue { get; set; } // Tổng giá trị nhập
        public decimal TotalExportValue { get; set; } // Tổng giá trị bán
        public decimal TotalProfit { get; set; }      // Tổng lợi nhuận
    }
    public class RevenueStatistic
    {
        public DateTime InvoiceDate { get; set; }

        public string Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
