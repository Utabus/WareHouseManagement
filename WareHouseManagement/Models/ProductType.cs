using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WareHouseManagement.Models
{
    public class ProductType
    {
        public int Id { get; set; }               // Khóa chính
        public string TypeCode { get; set; }      // Mã loại (vd: DT, LAP, PHUKIEN)
        public string TypeName { get; set; }      // Tên loại (vd: Điện thoại, Laptop, Phụ kiện)
        public string Description { get; set; }   // Mô tả thêm (nếu có)
    }
}
