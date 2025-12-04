using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WareHouseManagement.Models
{
    public class InvoiceDetailItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }

        // Product info
        public string ProductName { get; set; }
        public string Color { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public string Capacity { get; set; }

        // Detail info
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }

}
