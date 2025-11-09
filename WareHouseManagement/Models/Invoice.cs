using System;

namespace WareHouseManagement.Models
{
    public class Invoice
    {
        public int Id { get; set; }                     // Khóa chính
        public string InvoiceCode { get; set; }         // Mã hóa đơn (vd: HDX001)
        public DateTime InvoiceDate { get; set; }       // Ngày lập hóa đơn
        public string Type { get; set; }                // "Import" hoặc "Export"
        public decimal TotalAmount { get; set; }        // Tổng tiền hóa đơn
        public decimal Profit { get; set; }             // Lợi nhuận (Export mới có)
    }
}
