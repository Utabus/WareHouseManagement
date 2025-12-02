using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WareHouseManagement.Models
{
    public class Invoice: INotifyPropertyChanged
    {
        public int Id { get; set; }                     // Khóa chính
        public string InvoiceCode { get; set; }         // Mã hóa đơn (vd: HDX001)
        public DateTime InvoiceDate { get; set; }       // Ngày lập hóa đơn
        public string Type { get; set; }                // "Import" hoặc "Export"
        public string CustomerName { get; set; }

        public decimal TotalAmount { get; set; }        // Tổng tiền hóa đơn
        public decimal Profit { get; set; }             // Lợi nhuận (Export mới có)
        private bool isDebt;
        public bool IsDebt
        {
            get => isDebt;
            set
            {
                if (isDebt != value)
                {
                    isDebt = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDebt)));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
