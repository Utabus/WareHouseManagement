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

        // Command
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        public HoaDonViewModel()
        {
            // đúng thứ tự: hành động thực thi, điều kiện (có thể null)
            SearchCommand = new RelayCommand(_ => Search());
            RefreshCommand = new RelayCommand(_ => Refresh());

            LoadInvoices();
        }

        private void LoadInvoices()
        {
            var list = _db.GetInvoices().ToList();
            Invoices = new ObservableCollection<Invoice>(list);
        }

        private void Search()
        {
            var list = _db.GetInvoices();

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string lower = Keyword.Trim().ToLower();
                list = list.Where(x =>
                    x.InvoiceCode.ToLower().Contains(lower) ||
                    x.Type.ToLower().Contains(lower) ||
                    x.InvoiceDate.ToString("dd/MM/yyyy").Contains(lower)
                );
            }

            Invoices = new ObservableCollection<Invoice>(list);
        }

        private void Refresh()
        {
            Keyword = string.Empty;
            LoadInvoices();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
