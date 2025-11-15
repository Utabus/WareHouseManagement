using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WareHouseManagement.Models
{
    public class InvoiceProductInfo
    {
        public string ProductName { get; set; }
        public string Series { get; set; }
        public string InvoiceCode { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Type { get; set; }
    }

}
