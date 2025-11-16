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

        // Command
        public ICommand LoadRevenueStatisticsCommand { get; }
        public ICommand RefreshCommand { get; }

        public HoaDonViewModel()
        {
            // đúng thứ tự: hành động thực thi, điều kiện (có thể null)
            LoadRevenueStatisticsCommand = new RelayCommand(_ => Search());
            RefreshCommand = new RelayCommand(_ => Refresh());

            Search();
        }

        //private void LoadInvoices()
        //{
        //    var list = _db.GetInvoices().ToList();
        //    Invoices = new ObservableCollection<Invoice>(list);
        //}

        private void Search()
        {
            var list = _db.GetInvoicesByDate(FromDate, ToDate);
            Invoices = new ObservableCollection<Invoice>(list);
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
