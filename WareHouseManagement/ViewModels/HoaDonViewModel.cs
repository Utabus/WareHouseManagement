using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using WareHouseManagement.Data;
using WareHouseManagement.Models;

namespace WareHouseManagement.ViewModels
{
    public class HoaDonViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        private ObservableCollection<Invoice> _invoices;
        public ObservableCollection<Invoice> Invoices
        {
            get => _invoices;
            set
            {
                _invoices = value;
                OnPropertyChanged(nameof(Invoices));
            }
        }

        private string _keyword;
        public string Keyword
        {
            get => _keyword;
            set
            {
                _keyword = value;
                OnPropertyChanged(nameof(Keyword));
            }
        }
        private DateTime _fromDate = DateTime.Now.AddMonths(-1);
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged(nameof(FromDate));
                Search();

            }
        }

        private DateTime _toDate = DateTime.Now;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged(nameof(ToDate));
                Search();

            }
        }
        public decimal TongCongNo
        {
            get => Invoices?.Where(x => x.IsDebt).Sum(x => x.TotalAmount) ?? 0;
        }

        // Command
        public ICommand LoadRevenueStatisticsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CapNhatCongNoCommand { get; }
        public HoaDonViewModel()
        {
            // đúng thứ tự: hành động thực thi, điều kiện (có thể null)
            LoadRevenueStatisticsCommand = new RelayCommand(_ => Search());
            RefreshCommand = new RelayCommand(_ => Refresh());
            CapNhatCongNoCommand = new RelayCommand<Invoice>(invoice => invoice != null, UpdateDebt);
            Search();
        }

        //private void LoadInvoices()
        //{
        //    var list = _db.GetInvoices().ToList();
        //    Invoices = new ObservableCollection<Invoice>(list);
        //}

        private void UpdateDebt(Invoice invoice)
        {
            if (invoice == null) return;

            // Đổi trạng thái nợ
            bool newDebtStatus = !invoice.IsDebt;

            // Cập nhật vào DB
            bool success = _db.UpdateInvoiceDebt(invoice.Id, newDebtStatus);

            if (success)
            {
                invoice.IsDebt = newDebtStatus;
                OnPropertyChanged(nameof(TongCongNo));
                // Có thể hiển thị thông báo
                System.Windows.MessageBox.Show($"Hóa đơn {(newDebtStatus ? "công nợ" : "đã thanh toán")} thành công!",
                                               "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        private void Search()
        {
            var list = _db.GetInvoicesByDate(FromDate, ToDate);

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim().ToLower();

                list = list.Where(x =>
                      (!string.IsNullOrEmpty(x.CustomerName) && x.CustomerName.ToLower().Contains(kw))
                   || (!string.IsNullOrEmpty(x.InvoiceCode) && x.InvoiceCode.ToLower().Contains(kw))
                ).ToList();
            }

            Invoices = new ObservableCollection<Invoice>(list);

            OnPropertyChanged(nameof(TongCongNo));
        }

        private void Refresh()
        {
            Keyword = string.Empty;
            Search();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
