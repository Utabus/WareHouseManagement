namespace WareHouseManagement.Models
{
    public class InvoiceDetail
    {
        public int Id { get; set; }             // Khóa chính
        public int InvoiceId { get; set; }      // Khóa ngoại -> Invoice
        public int ProductId { get; set; }      // Khóa ngoại -> Product
        public int Quantity { get; set; }       // Số lượng
        public decimal UnitPrice { get; set; }  // Giá tại thời điểm giao dịch
        public decimal Total { get; set; }      // = Quantity * UnitPrice
    }
}
